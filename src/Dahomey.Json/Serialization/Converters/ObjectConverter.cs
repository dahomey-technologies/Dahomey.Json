﻿using System.Collections.Generic;
using System.Reflection;
using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Util;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using System.Text;
using Dahomey.Json.Serialization.Converters.Mappings;
using Dahomey.Json.Attributes;
using System.Linq;

namespace Dahomey.Json.Serialization.Converters
{
    public interface IObjectConverter
    {
        void ReadValue(ref Utf8JsonReader reader, object obj, ReadOnlySpan<byte> memberName, JsonSerializerOptions options, HashSet<IMemberConverter> readMembers);
        bool ReadValue(ref Utf8JsonReader reader, ReadOnlySpan<byte> memberName, JsonSerializerOptions options, HashSet<IMemberConverter> readMembers, out object value);
        IReadOnlyList<IMemberConverter> MemberConvertersForWrite { get; }
        IReadOnlyList<IMemberConverter> RequiredMemberConvertersForRead { get; }
        object CreateInstance();
    }

    public class ObjectConverter<T> : JsonConverter<T>, IObjectConverter
        where T : class
    {
        private class MemberConverters
        {
            public ByteBufferDictionary<IMemberConverter> ForRead = new ByteBufferDictionary<IMemberConverter>();
            public Dictionary<string, IMemberConverter> ForReadAsString;
            public List<IMemberConverter> RequiredForRead = new List<IMemberConverter>();
            public List<IMemberConverter> ForWrite = new List<IMemberConverter>();
            public IExtensionDataMemberConverter ExtensionData;

            public static MemberConverters Create(JsonSerializerOptions options, IObjectMapping objectMapping)
            {
                MemberConverters converters = new MemberConverters();

                if (options.PropertyNameCaseInsensitive)
                {
                    converters.ForReadAsString = new Dictionary<string, IMemberConverter>(StringComparer.OrdinalIgnoreCase);
                }

                converters.ExtensionData = objectMapping.ExtensionData;

                foreach (IMemberMapping memberMapping in objectMapping.MemberMappings)
                {
                    IMemberConverter memberConverter = memberMapping.GenerateMemberConverter();

                    if (memberMapping.CanBeDeserialized || objectMapping.IsCreatorMember(memberConverter.MemberName))
                    {
                        if (options.PropertyNameCaseInsensitive)
                        {
                            converters.ForReadAsString.Add(memberConverter.MemberNameAsString, memberConverter);
                        }
                        converters.ForRead.Add(memberConverter.MemberName, memberConverter);

                        if (memberConverter.RequirementPolicy == RequirementPolicy.AllowNull
                            || memberConverter.RequirementPolicy == RequirementPolicy.Always)
                        {
                            converters.RequiredForRead.Add(memberConverter);
                        }
                    }

                    if (memberMapping.CanBeSerialized)
                    {
                        converters.ForWrite.Add(memberConverter);
                    }
                }

                return converters;
            }
        }

        private readonly Lazy<MemberConverters> _memberConverters;
        private readonly IObjectMapping _objectMapping;
        private readonly Func<T> _constructor;
        private readonly bool _isInterfaceOrAbstract;
        private readonly IDiscriminatorConvention _discriminatorConvention;

        public IReadOnlyList<IMemberConverter> MemberConvertersForWrite => _memberConverters.Value.ForWrite;
        public IReadOnlyList<IMemberConverter> RequiredMemberConvertersForRead => _memberConverters.Value.RequiredForRead;

        public ObjectConverter(JsonSerializerOptions options)
        {
            _objectMapping = options.GetObjectMappingRegistry().Lookup<T>();
            _memberConverters = new Lazy<MemberConverters>(() => MemberConverters.Create(options, _objectMapping));

            _isInterfaceOrAbstract = typeof(T).IsInterface || typeof(T).IsAbstract;

            if (!_isInterfaceOrAbstract && _objectMapping.CreatorMapping == null)
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

            _discriminatorConvention = options.GetDiscriminatorConventionRegistry().GetConvention(typeof(T));
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

            Dictionary<ReadOnlyMemory<byte>, object> creatorValues = null;
            Dictionary<ReadOnlyMemory<byte>, object> regularValues = null;
            HashSet<IMemberConverter> readMembers = null;

            if (_objectMapping.CreatorMapping != null)
            {
                creatorValues = new Dictionary<ReadOnlyMemory<byte>, object>(ReadOnlyMemoryEqualityComparer<byte>.Instance);
                regularValues = new Dictionary<ReadOnlyMemory<byte>, object>(ReadOnlyMemoryEqualityComparer<byte>.Instance);
            }

            if (_memberConverters.Value.RequiredForRead.Count != 0)
            {
                readMembers = new HashSet<IMemberConverter>();
            }

            T obj = null;
            IObjectConverter converter = null;

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                ReadMember(ref reader, ref obj, ref converter, options, creatorValues, regularValues, readMembers);
            }

            if (creatorValues != null)
            {
                obj = (T)_objectMapping.CreatorMapping.CreateInstance(creatorValues);
                if (_objectMapping.OnDeserializingMethod != null)
                {
                    ((Action<T>)_objectMapping.OnDeserializingMethod)(obj);
                }

                foreach (KeyValuePair<ReadOnlyMemory<byte>, object> value in regularValues)
                {
                    if (!_memberConverters.Value.ForRead.TryGetValue(value.Key.Span, out IMemberConverter memberConverter))
                    {
                        // should not happen
                        throw new JsonException("Unexpected error");
                    }

                    memberConverter.Set(obj, value.Value, options);
                }
            }

            if (readMembers != null)
            {
                if (converter == null)
                {
                    converter = this;
                }

                foreach (IMemberConverter memberConverter in converter.RequiredMemberConvertersForRead)
                {
                    if (!readMembers.Contains(memberConverter))
                    {
                        throw new JsonException($"Required property '{memberConverter.MemberNameAsString}' not found in JSON.");
                    }
                }
            }

            if (_objectMapping.OnDeserializedMethod != null)
            {
                ((Action<T>)_objectMapping.OnDeserializedMethod)(obj);
            }

            return obj;
        }

        public void ReadMember(ref Utf8JsonReader reader, ref T obj, ref IObjectConverter converter, 
            JsonSerializerOptions options, Dictionary<ReadOnlyMemory<byte>, object> creatorValues,
            Dictionary<ReadOnlyMemory<byte>, object> regularValues,
            HashSet<IMemberConverter> readMembers)
        {
            ReadOnlySpan<byte> memberName = reader.GetRawString();
            reader.Read();

            if (obj == null)
            {
                if (converter == null)
                {
                    if (_discriminatorConvention != null)
                    {
                        if (memberName.SequenceEqual(_discriminatorConvention.MemberName))
                        {
                            // discriminator value
                            Type actualType = _discriminatorConvention.ReadDiscriminator(ref reader);

                            if (actualType == null)
                            {
                                throw new JsonException();
                            }

                            converter = (IObjectConverter)options.GetConverter(actualType);
                        }
                        else
                        {
                            converter = this;
                        }
                    }
                    else
                    {
                        converter = this;
                    }
                }

                if (creatorValues == null)
                {
                    obj = (T)converter.CreateInstance();

                    if (_objectMapping.OnDeserializingMethod != null)
                    {
                        ((Action<T>)_objectMapping.OnDeserializingMethod)(obj);
                    }

                    converter.ReadValue(ref reader, obj, memberName, options, readMembers);
                }
                else if (converter.ReadValue(ref reader, memberName, options, readMembers, out object value))
                {
                    if (_objectMapping.IsCreatorMember(memberName))
                    {
                        creatorValues.Add(memberName.ToArray(), value);
                    }
                    else
                    {
                        regularValues.Add(memberName.ToArray(), value);
                    }
                }
            }
            else
            {
                converter.ReadValue(ref reader, obj, memberName, options, readMembers);
            }
        }

        public void ReadValue(ref Utf8JsonReader reader, object obj, ReadOnlySpan<byte> memberName, JsonSerializerOptions options, HashSet<IMemberConverter> readMembers)
        {
            var memberConverters = _memberConverters.Value;

            if (options.PropertyNameCaseInsensitive 
                && memberConverters.ForReadAsString.TryGetValue(Encoding.UTF8.GetString(memberName), out IMemberConverter memberConverter)
                || !options.PropertyNameCaseInsensitive
                && memberConverters.ForRead.TryGetValue(memberName, out memberConverter))
            {
                if (readMembers != null)
                {
                    readMembers.Add(memberConverter);
                }
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

        public bool ReadValue(ref Utf8JsonReader reader, ReadOnlySpan<byte> memberName, JsonSerializerOptions options, HashSet<IMemberConverter> readMembers, out object value)
        {
            var memberConverters = _memberConverters.Value;

            if (options.PropertyNameCaseInsensitive
                && memberConverters.ForReadAsString.TryGetValue(Encoding.UTF8.GetString(memberName), out IMemberConverter memberConverter)
                || !options.PropertyNameCaseInsensitive
                && memberConverters.ForRead.TryGetValue(memberName, out memberConverter))
            {
                if (readMembers != null)
                {
                    readMembers.Add(memberConverter);
                }
                value = memberConverter.Read(ref reader, options);
                return true;
            }
            else
            {
                reader.Skip();
                value = default;
                return false;
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (_objectMapping.OnSerializingMethod != null)
            {
                ((Action<T>)_objectMapping.OnSerializingMethod)(value);
            }

            writer.WriteStartObject();

            Type declaredType = typeof(T);
            Type actualType = value.GetType();

            IReadOnlyList<IMemberConverter> memberConvertersForWrite;

            if (_objectMapping.CreatorMapping == null && actualType != declaredType)
            {
                IObjectConverter converter = (IObjectConverter)options.GetConverter(actualType);
                memberConvertersForWrite = converter.MemberConvertersForWrite;
            }
            else
            {
                memberConvertersForWrite = _memberConverters.Value.ForWrite;
            }

            foreach (IMemberConverter memberConverter in memberConvertersForWrite)
            {
                if (memberConverter.ShouldSerialize(value, typeof(T), options))
                {
                    writer.WritePropertyName(memberConverter.MemberName);
                    memberConverter.Write(writer, value, options);
                }
            }

            writer.WriteEndObject();

            if (_objectMapping.OnSerializedMethod != null)
            {
                ((Action<T>)_objectMapping.OnSerializedMethod)(value);
            }
        }
    }
}
