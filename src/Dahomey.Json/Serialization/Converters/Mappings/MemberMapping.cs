using Dahomey.Json.Attributes;
using Dahomey.Json.Util;
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class MemberMapping<T> : IMemberMapping
    {
        private readonly IObjectMapping _objectMapping;
        private readonly JsonSerializerOptions _options;
        private bool _forceDeserialize = false;

        public MemberInfo MemberInfo { get; private set; }
        public Type MemberType { get; private set; }
        public string? MemberName { get; set; }
        public JsonConverter? Converter { get; set; }
        public bool CanBeDeserialized { get; private set; }
        public bool CanBeSerialized { get; private set; }
        public object? DefaultValue { get; set; }
        public bool IgnoreIfDefault { get; set; }
        public Func<object, bool>? ShouldSerializeMethod { get; set; }
        public RequirementPolicy RequirementPolicy { get; set; }

        public MemberMapping(JsonSerializerOptions options,
            IObjectMapping objectMapping, MemberInfo memberInfo, Type memberType)
        {
            _objectMapping = objectMapping;
            _options = options;
            MemberInfo = memberInfo;
            MemberType = memberType;
            DefaultValue = Default.Value(memberType);
        }

        public MemberMapping<T> SetMemberName(string memberName)
        {
            MemberName = memberName;
            return this;
        }

        public MemberMapping<T> SetConverter(JsonConverter converter)
        {
            Converter = converter;
            return this;
        }

        public MemberMapping<T> SetDefaultValue(object? defaultValue)
        {
            DefaultValue = defaultValue;
            return this;
        }

        public MemberMapping<T> SetIgnoreIfDefault(bool ignoreIfDefault)
        {
            IgnoreIfDefault = ignoreIfDefault;
            return this;
        }

        public MemberMapping<T> SetShouldSerializeMethod(Func<object, bool> shouldSerializeMethod)
        {
            ShouldSerializeMethod = shouldSerializeMethod;
            return this;
        }

        public MemberMapping<T> SetRequired(RequirementPolicy requirementPolicy)
        {
            RequirementPolicy = requirementPolicy;
            return this;
        }

        public MemberMapping<T> ForceDeserialize()
        {
            _forceDeserialize = true;
            return this;
        }

        public void Initialize()
        {
            InitializeMemberName();
            InitializeCanBeDeserialized();
            InitializeCanBeSerialized();
            ValidateDefaultValue();
        }

        public void PostInitialize()
        {
        }

        public IMemberConverter GenerateMemberConverter()
        {
            InitializeConverter();

            if (typeof(T).IsStruct())
            {
                Type structMemberConverterType = typeof(StructMemberConverter<,>).MakeGenericType(typeof(T), MemberType);
                IMemberConverter? structMemberConverter =
                    (IMemberConverter?)Activator.CreateInstance(structMemberConverterType, _options, this);

                if (structMemberConverter == null)
                {
                    throw new JsonException($"Cannot instantiate {structMemberConverterType}");
                }

                return structMemberConverter;
            }

            Type memberConverterType = typeof(MemberConverter<,>).MakeGenericType(typeof(T), MemberType);
            IMemberConverter? memberConverter =
                (IMemberConverter?)Activator.CreateInstance(memberConverterType, _options, this);

            if (memberConverter == null)
            {
                throw new JsonException($"Cannot instantiate {memberConverterType}");
            }

            return memberConverter;
        }

        private void InitializeMemberName()
        {
            if (string.IsNullOrEmpty(MemberName))
            {
                if (_objectMapping.PropertyNamingPolicy != null)
                {
                    MemberName = _objectMapping.PropertyNamingPolicy.ConvertName(MemberInfo.Name);
                }
                else if (_options.PropertyNamingPolicy != null)
                {
                    MemberName = _options.PropertyNamingPolicy.ConvertName(MemberInfo.Name) ?? throw new JsonException();
                }
                else
                {
                    MemberName= MemberInfo.Name;
                }
            }
        }

        private void InitializeConverter()
        {
            if (Converter == null)
            {
                Converter = _options.GetConverter(MemberType);
            }
            VerifyMemberConverterType(Converter.GetType());
        }

        private void InitializeCanBeDeserialized()
        {
            switch (MemberInfo)
            {
                case PropertyInfo propertyInfo:
                    CanBeDeserialized = (propertyInfo.CanWrite
                        || _options.GetReadOnlyPropertyHandling() == ReadOnlyPropertyHandling.Read
                            || (_forceDeserialize && _options.GetReadOnlyPropertyHandling() == ReadOnlyPropertyHandling.Default))
                        && propertyInfo.GetMethod != null && !propertyInfo.GetMethod.IsStatic;
                    break;

                case FieldInfo fieldInfo:
                    CanBeDeserialized = !fieldInfo.IsInitOnly && !fieldInfo.IsStatic;
                    break;

                default:
                    CanBeDeserialized = false;
                    break;
            }
        }

        private void InitializeCanBeSerialized()
        {
            switch (MemberInfo)
            {
                case PropertyInfo propertyInfo:
                    CanBeSerialized = propertyInfo.CanRead && (!_options.IgnoreReadOnlyProperties || propertyInfo.CanWrite);
                    break;

                case FieldInfo fieldInfo:
                    CanBeSerialized = true;
                    break;

                default:
                    CanBeSerialized = false;
                    break;
            }
        }

        private void VerifyMemberConverterType(Type memberConverterType)
        {
            Type jsonConverterType = typeof(JsonConverter<>).MakeGenericType(MemberType);
            if (!jsonConverterType.IsAssignableFrom(memberConverterType))
            {
                throw new JsonException($"Custom converter on member {MemberInfo.ReflectedType?.Name}.{MemberInfo.Name} is not a JsonConverter<{MemberType.Name}>");
            }
        }

        private void ValidateDefaultValue()
        {
            if ((DefaultValue == null && MemberType.IsValueType && Nullable.GetUnderlyingType(MemberType) == null)
                || (DefaultValue != null && DefaultValue.GetType() != MemberType))
            {
                throw new JsonException($"Default value type mismatch");
            }
        }
    }
}
