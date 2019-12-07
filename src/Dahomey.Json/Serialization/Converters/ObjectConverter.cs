using System.Collections.Generic;
using System.Reflection;
using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Util;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using System.Text;
using System.Buffers;

namespace Dahomey.Json.Serialization.Converters
{
    public interface IObjectConverter
    {
        IReadOnlyList<IMemberConverter> MemberConvertersForWrite { get; }
        object CreateInstance();
        void ReadValue(ref Utf8JsonReader reader, object obj, ReadOnlySpan<byte> memberName, JsonSerializerOptions options);
    }

    public class ObjectConverter<T> : JsonConverter<T>, IObjectConverter
        where T : class
    {
        private readonly Func<T> _constructor;
        private readonly bool _isInterfaceOrAbstract;
        private readonly IDiscriminatorConvention _discriminatorConvention;

        private class MemberConverters
        {
            public ByteBufferDictionary<IMemberConverter> ForRead = new ByteBufferDictionary<IMemberConverter>();
            public Dictionary<string, IMemberConverter> ForReadAsString;
            public List<IMemberConverter> ForWrite = new List<IMemberConverter>();
            public IExtensionDataMemberConverter ExtensionData;

            public static MemberConverters Create(JsonSerializerOptions options)
            {
                MemberConverters converters = new MemberConverters();

                if (options.PropertyNameCaseInsensitive)
                {
                    converters.ForReadAsString = new Dictionary<string, IMemberConverter>(StringComparer.OrdinalIgnoreCase);
                }

                foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
                {
                    if (propertyInfo.IsDefined(typeof(JsonIgnoreAttribute)))
                    {
                        continue;
                    }

                    if (propertyInfo.IsDefined(typeof(JsonExtensionDataAttribute)))
                    {
                        Type propertyType = propertyInfo.PropertyType;
                        if (propertyType != typeof(Dictionary<string, object>)
                            && propertyType != typeof(Dictionary<string, JsonElement>))
                        {
                            throw new JsonException("Invalid Serialization DataExtension Property");
                        }

                        converters.ExtensionData = (IExtensionDataMemberConverter)
                            Activator.CreateInstance(typeof(ExtensionDataMemberConverter<,>)
                                .MakeGenericType(typeof(T), propertyType.GetGenericArguments()[1]),
                                propertyInfo, options);

                        continue;
                    }

                    IMemberConverter memberConverter = (IMemberConverter)Activator.CreateInstance(
                        typeof(MemberConverter<,>).MakeGenericType(typeof(T), propertyInfo.PropertyType),
                        propertyInfo, options);

                    if (propertyInfo.CanWrite)
                    {
                        if (options.PropertyNameCaseInsensitive)
                        {
                            converters.ForReadAsString.Add(memberConverter.MemberNameAsString, memberConverter);
                        }
                        converters.ForRead.Add(memberConverter.MemberName, memberConverter);
                    }

                    if (propertyInfo.CanRead && (!options.IgnoreReadOnlyProperties || propertyInfo.CanWrite))
                    {
                        converters.ForWrite.Add(memberConverter);
                    }
                }

                return converters;
            }
        }

        private readonly Lazy<MemberConverters> _memberConverters;
        private readonly DiscriminatorPolicy _discriminatorPolicy;

        public IReadOnlyList<IMemberConverter> MemberConvertersForWrite => _memberConverters.Value.ForWrite;

        public ObjectConverter(JsonSerializerOptions options)
        {
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            _discriminatorConvention = registry.GetConvention(typeof(T));
            _discriminatorPolicy = registry.DiscriminatorPolicy;
            _memberConverters = new Lazy<MemberConverters>(() => MemberConverters.Create(options));

            _isInterfaceOrAbstract = typeof(T).IsInterface || typeof(T).IsAbstract;

            if (!_isInterfaceOrAbstract)
            {
                ConstructorInfo defaultConstructorInfo = typeof(T).GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    Type.EmptyTypes,
                    null);

                if (defaultConstructorInfo == null)
                {
                    throw new JsonException($"Cannot find a default constructor on type {typeof(T)}");
                }

                _constructor = defaultConstructorInfo.CreateDelegate<T>();
            }
        }

        public object CreateInstance()
        {
            if (_isInterfaceOrAbstract)
            {
                throw new JsonException("Cannot instantiate an interface nor an abstract classes");
            }

            return _constructor();
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            ReadOnlySpan<byte> memberName = reader.GetRawString();
            reader.Read();

            T obj;
            IObjectConverter converter;

            if (_discriminatorConvention != null && 
                memberName.SequenceEqual(_discriminatorConvention.MemberName))
            {
                // discriminator value
                Type actualType = _discriminatorConvention.ReadDiscriminator(ref reader);

                if (actualType == null)
                {
                    throw new JsonException();
                }

                converter = (IObjectConverter)options.GetConverter(actualType);
                obj = (T)converter.CreateInstance();
            }
            else
            {
                converter = this;
                obj = (T)converter.CreateInstance();

                converter.ReadValue(ref reader, obj, memberName, options);
            }

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                memberName = reader.GetRawString();
                reader.Read();

                converter.ReadValue(ref reader, obj, memberName, options);
            }

            return obj;
        }

        public void ReadValue(ref Utf8JsonReader reader, object obj, ReadOnlySpan<byte> memberName, JsonSerializerOptions options)
        {
            var memberConverters = _memberConverters.Value;

            if (options.PropertyNameCaseInsensitive 
                && memberConverters.ForReadAsString.TryGetValue(Encoding.UTF8.GetString(memberName), out IMemberConverter memberConverter)
                || !options.PropertyNameCaseInsensitive
                && memberConverters.ForRead.TryGetValue(memberName, out memberConverter))
            {
                memberConverter.Read(ref reader, obj, options);
            }
            else if (memberConverters.ExtensionData != null)
            {
                memberConverters.ExtensionData.Read(ref reader, obj, memberName, options);
            }
            else
            {
                reader.Skip();
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            Type declaredType = typeof(T);
            Type actualType = value.GetType();

            IReadOnlyList<IMemberConverter> memberConvertersForWrite;

            if (actualType != declaredType)
            {
                IObjectConverter converter = (IObjectConverter)options.GetConverter(actualType);
                memberConvertersForWrite = converter.MemberConvertersForWrite;
            }
            else
            {
                memberConvertersForWrite = _memberConverters.Value.ForWrite;
            }

            if (_discriminatorConvention != null &&
                (_discriminatorPolicy == DiscriminatorPolicy.Always
                || _discriminatorPolicy == DiscriminatorPolicy.Auto && actualType != declaredType))
            {
                writer.WritePropertyName(_discriminatorConvention.MemberName);
                _discriminatorConvention.WriteDiscriminator(writer, actualType);
            }

            foreach (IMemberConverter memberConverter in memberConvertersForWrite)
            {
                if (memberConverter.ShouldSerialize(value, options))
                {
                    writer.WritePropertyName(memberConverter.MemberName);
                    memberConverter.Write(writer, value, options);
                }
            }

            writer.WriteEndObject();
        }
    }
}
