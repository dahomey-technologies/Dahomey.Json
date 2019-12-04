using System.Collections.Generic;
using System.Reflection;
using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Util;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using System.Text;

namespace Dahomey.Json.Serialization.Converters
{
    public interface IObjectConverter
    {
        IReadOnlyList<IMemberConverter> MemberConvertersForWrite { get; }
        object CreateInstance();
        void ReadValue(ref Utf8JsonReader reader, object obj, ReadOnlySpan<byte> memberName, JsonSerializerOptions options);
    }

    public class ObjectConverter<T> : JsonConverter<T>, IObjectConverter
        where T : class, new()
    {
        private readonly IDiscriminatorConvention _discriminatorConvention;

        private class MemberConverters
        {
            public ByteBufferDictionary<IMemberConverter> ForRead = new ByteBufferDictionary<IMemberConverter>();
            public Dictionary<string, IMemberConverter> ForReadAsString;
            public List<IMemberConverter> ForWrite = new List<IMemberConverter>();

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
        private readonly ReadOnlyMemory<byte> _discriminatorValue;
        private readonly DiscriminatorPolicy _discriminatorPolicy;

        public IReadOnlyList<IMemberConverter> MemberConvertersForWrite => _memberConverters.Value.ForWrite;
        public ReadOnlySpan<byte> DiscriminatorValue => _discriminatorValue.Span;

        public ObjectConverter(JsonSerializerOptions options)
        {
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            _discriminatorConvention = registry.GetConvention(typeof(T));
            _discriminatorPolicy = registry.DiscriminatorPolicy;
            _memberConverters = new Lazy<MemberConverters>(() => MemberConverters.Create(options));
        }

        public object CreateInstance()
        {
            return new T();
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

            ReadOnlySpan<byte> memberName = reader.ValueSpan;
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

                memberName = reader.ValueSpan;
                reader.Read();

                converter.ReadValue(ref reader, obj, memberName, options);
            }

            return obj;
        }

        public void ReadValue(ref Utf8JsonReader reader, object obj, ReadOnlySpan<byte> memberName, JsonSerializerOptions options)
        {
            if (options.PropertyNameCaseInsensitive 
                && _memberConverters.Value.ForReadAsString.TryGetValue(Encoding.UTF8.GetString(memberName), out IMemberConverter memberConverter)
                || !options.PropertyNameCaseInsensitive
                && _memberConverters.Value.ForRead.TryGetValue(memberName, out memberConverter))
            {
                memberConverter.Read(ref reader, obj, options);
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
