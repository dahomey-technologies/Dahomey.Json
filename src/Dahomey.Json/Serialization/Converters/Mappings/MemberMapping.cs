using Dahomey.Json.Attributes;
using Dahomey.Json.Util;
using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class MemberMapping<T> : IMemberMapping
    {
        private readonly IObjectMapping _objectMapping;
        private readonly JsonSerializerOptions _options;

        public MemberInfo MemberInfo { get; private set; }
        public Type MemberType { get; private set; }
        public string MemberName { get; private set; }
        public JsonConverter Converter { get; private set; }
        public bool CanBeDeserialized { get; private set; }
        public bool CanBeSerialized { get; private set; }
        public object DefaultValue { get; private set; }
        public bool IgnoreIfDefault { get; private set; }
        public Func<object, bool> ShouldSerializeMethod { get; private set; }
        public RequirementPolicy RequirementPolicy { get; private set; }

        public MemberMapping(JsonSerializerOptions options,
            IObjectMapping objectMapping, MemberInfo memberInfo, Type memberType)
        {
            _objectMapping = objectMapping;
            _options = options;
            MemberInfo = memberInfo;
            MemberType = memberType;
            DefaultValue = (memberType.IsClass || memberType.IsInterface) ? null : Activator.CreateInstance(memberType);
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

        public MemberMapping<T> SetDefaultValue(object defaultValue)
        {
            DefaultValue = defaultValue;
            return this;
        }

        public MemberMapping<T> SetIngoreIfDefault(bool ignoreIfDefault)
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
                return (IMemberConverter)Activator.CreateInstance(
                    typeof(StructMemberConverter<,>).MakeGenericType(typeof(T), MemberType),
                    _options, this);
            }

            return (IMemberConverter)Activator.CreateInstance(
            typeof(MemberConverter<,>).MakeGenericType(typeof(T), MemberType),
            _options, this);
        }

        private void InitializeMemberName()
        {
            if (string.IsNullOrEmpty(MemberName))
            {
                JsonPropertyNameAttribute nameAttribute = MemberInfo.GetCustomAttribute<JsonPropertyNameAttribute>(inherit: false);
                DataMemberAttribute dataMemberAttribute = MemberInfo.GetCustomAttribute<DataMemberAttribute>(inherit: false);
                if (nameAttribute != null)
                {
                    MemberName = nameAttribute.Name ?? throw new JsonException();
                }
                else if (dataMemberAttribute != null)
                {
                    MemberName = dataMemberAttribute.Name;
                }
                else if (_objectMapping.PropertyNamingPolicy != null)
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
                JsonConverterAttribute converterAttribute = MemberInfo.GetCustomAttribute<JsonConverterAttribute>();
                if (converterAttribute != null)
                {
                    Type converterType = converterAttribute.ConverterType;
                    VerifyMemberConverterType(converterType);

                    Converter = (JsonConverter)Activator.CreateInstance(converterType);
                }
                else
                {
                    Converter = _options.GetConverter(MemberType);
                }
            }
            else
            {
                VerifyMemberConverterType(Converter.GetType());
            }
        }

        private void InitializeCanBeDeserialized()
        {
            switch (MemberInfo)
            {
                case PropertyInfo propertyInfo:
                    CanBeDeserialized = propertyInfo.CanWrite && !propertyInfo.GetMethod.IsStatic;
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
                throw new JsonException($"Custom converter on member {MemberInfo.ReflectedType.Name}.{MemberInfo.Name} is not a JsonConverter<{MemberType.Name}>");
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
