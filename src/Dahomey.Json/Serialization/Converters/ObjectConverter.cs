using System.Collections.Generic;
using System.Reflection;
using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Util;
using System.Text.Json;
using System;
using System.Text;
using Dahomey.Json.Serialization.Converters.Mappings;
using Dahomey.Json.Attributes;

namespace Dahomey.Json.Serialization.Converters
{
    public interface IObjectConverter
    {
        IReadOnlyList<IMemberConverter> MemberConvertersForWrite { get; }
        IReadOnlyList<IMemberConverter> RequiredMemberConvertersForRead { get; }
        IObjectMapping ObjectMapping { get; }
        object CreateInstance();
        void ReadValue(ref Utf8JsonReader reader, object obj, ReadOnlySpan<byte> memberName, JsonSerializerOptions options, HashSet<IMemberConverter> readMembers);
        bool ReadValue(ref Utf8JsonReader reader, ReadOnlySpan<byte> memberName, JsonSerializerOptions options, HashSet<IMemberConverter>? readMembers, out object value);
    }

    public class ObjectConverter<T> : AbstractJsonConverter<T>, IObjectConverter
    {
        private class MemberConverters
        {
            public ByteBufferDictionary<IMemberConverter> ForRead = new ByteBufferDictionary<IMemberConverter>();
            public Dictionary<string, IMemberConverter>? ForReadAsString;
            public List<IMemberConverter> RequiredForRead = new List<IMemberConverter>();
            public List<IMemberConverter> ForWrite = new List<IMemberConverter>();
            public IExtensionDataMemberConverter? ExtensionData;

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
                            converters.ForReadAsString!.Add(memberConverter.MemberNameAsString!, memberConverter);
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
        private readonly Func<T>? _constructor;
        private readonly bool _isInterfaceOrAbstract;
        private readonly bool _isStruct;
        private readonly IDiscriminatorConvention? _discriminatorConvention;
        private readonly ReferenceHandling _referenceHandling;
        private readonly MissingMemberHandling _missingMemberHandling;

        public IReadOnlyList<IMemberConverter> MemberConvertersForWrite => _memberConverters.Value.ForWrite;
        public IReadOnlyList<IMemberConverter> RequiredMemberConvertersForRead => _memberConverters.Value.RequiredForRead;
        public IObjectMapping ObjectMapping => _objectMapping;


        public ObjectConverter(JsonSerializerOptions options)
        {
            _objectMapping = options.GetObjectMappingRegistry().Lookup<T>();
            _memberConverters = new Lazy<MemberConverters>(() => MemberConverters.Create(options, _objectMapping));

            _isInterfaceOrAbstract = typeof(T).IsInterface || typeof(T).IsAbstract;
            _isStruct = typeof(T).IsStruct();

            if (!_isInterfaceOrAbstract && _objectMapping.CreatorMapping == null && !_isStruct)
            {
                ConstructorInfo? defaultConstructorInfo = typeof(T).GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    Type.EmptyTypes,
                    null);

                if (defaultConstructorInfo != null)
                {
                    _constructor = defaultConstructorInfo.CreateDelegate<T>();
                }
            }

            _discriminatorConvention = options.GetDiscriminatorConventionRegistry().GetConvention(typeof(T));
            _referenceHandling = _isStruct ? ReferenceHandling.Default : options.GetReferenceHandling();
            _missingMemberHandling = options.GetMissingMemberHandling();
        }

        public object CreateInstance()
        {
            if (_isInterfaceOrAbstract || _constructor == null)
            {
                throw new JsonException("Cannot instantiate an interface nor an abstract classes");
            }

            return _constructor()!;
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default!;
            }

            T obj = default!;

            Read(ref reader, ref obj, options);

            return obj;
        }

        public override void Read(ref Utf8JsonReader reader, ref T obj, JsonSerializerOptions options)
        {
            using (new DepthHandler(options))
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected start of object");
                }

                Dictionary<ReadOnlyMemory<byte>, object>? creatorValues = null;
                Dictionary<ReadOnlyMemory<byte>, object>? regularValues = null;
                HashSet<IMemberConverter>? readMembers = null;

                if (_memberConverters.Value.RequiredForRead.Count != 0)
                {
                    readMembers = new HashSet<IMemberConverter>();
                }

                IObjectConverter? converter = null;

                string? id = null;
                bool end = false;

                if (_referenceHandling == ReferenceHandling.Preserve)
                {
                    reader.Read();

                    switch(reader.TokenType)
                    {
                        case JsonTokenType.EndObject:
                            end = true;
                            break;

                        case JsonTokenType.PropertyName:
                            {
                                ReadOnlySpan<byte> memberName = reader.GetRawString();

                                if (memberName.SequenceEqual(ReferenceHandler.ID_MEMBER_NAME))
                                {
                                    reader.Read();
                                    id = reader.GetString();
                                }
                                else if (memberName.SequenceEqual(ReferenceHandler.REF_MEMBER_NAME))
                                {
                                    reader.Read();
                                    string? @ref = reader.GetString();

                                    if (@ref == null)
                                    {
                                        throw new JsonException($"Cannot resolve null reference");
                                    }

                                    object? @object = SerializationContext.Current.ReferenceHandler.ResolveReference(@ref);

                                    if (@object == null)
                                    {
                                        throw new JsonException($"Cannot resolve reference {@ref}");
                                    }

                                    reader.Read();
                                    if (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        throw new JsonException("Expected end of object");
                                    }

                                    obj = (T)@object;
                                    return;
                                }
                                else
                                {
                                    ReadMember(ref reader, ref obj, ref converter, options, ref creatorValues, ref regularValues, readMembers, id);
                                }

                                break;
                            }
                        default:
                            throw new JsonException();
                    }
                }

                if (!end)
                {
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName)
                        {
                            throw new JsonException();
                        }

                        ReadMember(ref reader, ref obj, ref converter, options, ref creatorValues, ref regularValues, readMembers, id);
                    }
                }

                if (creatorValues != null && converter != null)
                {
                    obj = (T)converter.ObjectMapping.CreatorMapping!.CreateInstance(creatorValues);

                    if (!string.IsNullOrEmpty(id))
                    {
                        SerializationContext.Current.ReferenceHandler.AddReference(obj, id);
                    }

                    if (converter.ObjectMapping.OnDeserializingMethod != null)
                    {
                        ((Action<T>)converter.ObjectMapping.OnDeserializingMethod)(obj);
                    }

                    foreach (KeyValuePair<ReadOnlyMemory<byte>, object> value in regularValues!)
                    {
                        if (!_memberConverters.Value.ForRead.TryGetValue(value.Key.Span, out IMemberConverter? memberConverter))
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

                if (obj == null)
                {
                    if (converter == null)
                    {
                        converter = this;
                    }

                    obj = (T)converter.CreateInstance();

                    if (!string.IsNullOrEmpty(id))
                    {
                        SerializationContext.Current.ReferenceHandler.AddReference(obj, id);
                    }
                }

                if (_objectMapping.OnDeserializedMethod != null)
                {
                    ((Action<T>)_objectMapping.OnDeserializedMethod)(obj);
                }
            }
        }

        private void ReadMember(ref Utf8JsonReader reader, ref T obj, ref IObjectConverter? converter, 
            JsonSerializerOptions options, ref Dictionary<ReadOnlyMemory<byte>, object>? creatorValues,
            ref Dictionary<ReadOnlyMemory<byte>, object>? regularValues,
            HashSet<IMemberConverter>? readMembers, string? id)
        {
            if (obj == null || converter == null)
            {
                if (converter == null)
                {
                    if (_discriminatorConvention != null)
                    {
                        // make a copy, to preserve the state of "reader"
                        Utf8JsonReader findReader = reader;

                        if (FindItem(ref findReader, _discriminatorConvention.MemberName))
                        {
                            // discriminator value
                            Type actualType = _discriminatorConvention.ReadDiscriminator(ref findReader);

                            if (!_objectMapping.ObjectType.IsAssignableFrom(actualType))
                            {
                                throw new JsonException($"expected type {_objectMapping.ObjectType} is not assignable from actual type {actualType}");
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

                    if (converter.ObjectMapping.CreatorMapping != null)
                    {
                        creatorValues = new Dictionary<ReadOnlyMemory<byte>, object>(ReadOnlyMemoryEqualityComparer<byte>.Instance);
                        regularValues = new Dictionary<ReadOnlyMemory<byte>, object>(ReadOnlyMemoryEqualityComparer<byte>.Instance);
                    }
                }

                if (creatorValues == null)
                {
                    if (!_isStruct && obj == null)
                    {
                        obj = (T)converter.CreateInstance();

                        if (!string.IsNullOrEmpty(id))
                        {
                            SerializationContext.Current.ReferenceHandler.AddReference(obj, id);
                        }
                    }

                    if (converter.ObjectMapping.OnDeserializingMethod != null)
                    {
                        ((Action<T>)converter.ObjectMapping.OnDeserializingMethod)(obj);
                    }
                }
            }

            ReadOnlySpan<byte> memberName = reader.GetRawString();
            reader.Read();

            if (creatorValues == null)
            {
                if (_isStruct)
                {
                    ReadValueForStruct(ref reader, ref obj, memberName, options, readMembers!);
                }
                else
                {
                    converter.ReadValue(ref reader, obj!, memberName, options, readMembers!);
                }
            }
            else if (converter.ReadValue(ref reader, memberName, options, readMembers, out object value))
            {
                if (converter.ObjectMapping.IsCreatorMember(memberName))
                {
                    creatorValues.Add(memberName.ToArray(), value);
                }
                else
                {
                    regularValues!.Add(memberName.ToArray(), value);
                }
            }
        }

        private bool FindItem(ref Utf8JsonReader reader, ReadOnlySpan<byte> name)
        {
            do
            {
                ReadOnlySpan<byte> memberName = reader.GetRawString();
                reader.Read();

                if (memberName.SequenceEqual(name))
                {
                    return true;
                }

                reader.Skip();
                reader.Read();
            }
            while (reader.TokenType == JsonTokenType.PropertyName);

            return false;
        }

        private void ReadValueForStruct(ref Utf8JsonReader reader, ref T instance, ReadOnlySpan<byte> memberName, JsonSerializerOptions options, HashSet<IMemberConverter> readMembers)
        {
            var memberConverters = _memberConverters.Value;

            if (options.PropertyNameCaseInsensitive
                && memberConverters.ForReadAsString != null && memberConverters.ForReadAsString.TryGetValue(Encoding.UTF8.GetString(memberName), out IMemberConverter? memberConverter)
                || !options.PropertyNameCaseInsensitive
                && memberConverters.ForRead.TryGetValue(memberName, out memberConverter))
            {
                if (readMembers != null)
                {
                    readMembers.Add(memberConverter);
                }

                ((IMemberConverter<T>)memberConverter).Read(ref reader, ref instance, options);
            }
            else if (_missingMemberHandling == MissingMemberHandling.Error)
            {
                throw new JsonException($"Missing member '{Encoding.UTF8.GetString(memberName)}'");
            }
            else if (memberConverters.ExtensionData != null)
            {
                memberConverters.ExtensionData.Read(ref reader, instance!, memberName, options);
            }
            else
            {
                reader.Skip();
            }
        }

        public void ReadValue(ref Utf8JsonReader reader, object obj, ReadOnlySpan<byte> memberName, JsonSerializerOptions options, HashSet<IMemberConverter> readMembers)
        {
            var memberConverters = _memberConverters.Value;

            if (options.PropertyNameCaseInsensitive
                && memberConverters.ForReadAsString != null && memberConverters.ForReadAsString.TryGetValue(Encoding.UTF8.GetString(memberName), out IMemberConverter? memberConverter)
                || !options.PropertyNameCaseInsensitive
                && memberConverters.ForRead.TryGetValue(memberName, out memberConverter))
            {
                if (readMembers != null)
                {
                    readMembers.Add(memberConverter);
                }

                memberConverter.Read(ref reader, obj, options);
            }
            else if(_discriminatorConvention != null && memberName.SequenceEqual(_discriminatorConvention.MemberName))
            {
                reader.Skip();
            }
            else if (_missingMemberHandling == MissingMemberHandling.Error)
            {
                throw new JsonException($"Could not find member '{Encoding.UTF8.GetString(memberName)}' on object of type '{obj.GetType().Name}'");
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

        public bool ReadValue(ref Utf8JsonReader reader, ReadOnlySpan<byte> memberName, JsonSerializerOptions options, HashSet<IMemberConverter>? readMembers, out object value)
        {
            var memberConverters = _memberConverters.Value;

            if (options.PropertyNameCaseInsensitive
                && memberConverters.ForReadAsString != null && memberConverters.ForReadAsString.TryGetValue(Encoding.UTF8.GetString(memberName), out IMemberConverter? memberConverter)
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
                value = default!;
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

            using (new DepthHandler(options))
            {
                if (_objectMapping.OnSerializingMethod != null)
                {
                    ((Action<T>)_objectMapping.OnSerializingMethod)(value);
                }

                writer.WriteStartObject();

                if (_referenceHandling == ReferenceHandling.Ignore)
                {
                    ReferenceHandler referenceResolver = SerializationContext.Current.ReferenceHandler;

                    if (referenceResolver.IsReferenced(value))
                    {
                        writer.WriteNullValue();
                        return;
                    }
                    else
                    {
                        referenceResolver.AddReference(value);
                    }
                }
                else if (_referenceHandling == ReferenceHandling.Preserve)
                {
                    ReferenceHandler referenceResolver = SerializationContext.Current.ReferenceHandler;
                    string reference = referenceResolver.GetReference(value, out bool firstReference);

                    if (firstReference)
                    {
                        writer.WriteString(ReferenceHandler.ID_MEMBER_NAME, reference);
                    }
                    else
                    {
                        writer.WriteString(ReferenceHandler.REF_MEMBER_NAME, reference);
                        writer.WriteEndObject();
                        return;
                    }
                }

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
                    if (_isStruct)
                    {
                        IMemberConverter<T> typedMemberConverter = (IMemberConverter<T>)memberConverter;

                        if (typedMemberConverter.ShouldSerialize(ref value, typeof(T), options))
                        {
                            writer.WritePropertyName(memberConverter.MemberName);
                            typedMemberConverter.Write(writer, ref value, options);
                        }
                    }
                    else
                    {
                        if (memberConverter.ShouldSerialize(value!, typeof(T), options))
                        {
                            writer.WritePropertyName(memberConverter.MemberName);
                            memberConverter.Write(writer, value!, options);
                        }
                    }
                }

                var memberConverters = _memberConverters.Value;
                if (memberConverters.ExtensionData != null)
                {
                    memberConverters.ExtensionData.Write(writer, value!, options);
                }
                
                writer.WriteEndObject();

                if (_objectMapping.OnSerializedMethod != null)
                {
                    ((Action<T>)_objectMapping.OnSerializedMethod)(value);
                }
            }
        }
    }
}
