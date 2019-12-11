using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Serialization.Converters.Mappings;
using Dahomey.Json.Util;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public interface IMemberConverter
    {
        ReadOnlySpan<byte> MemberName { get; }
        bool IgnoreIfDefault { get; }
        string MemberNameAsString { get; }
        RequirementPolicy RequirementPolicy { get; }

        void Read(ref Utf8JsonReader reader, object obj, JsonSerializerOptions options);
        void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options);
        object Read(ref Utf8JsonReader reader, JsonSerializerOptions options);
        void Set(object obj, object value, JsonSerializerOptions options);
        bool ShouldSerialize(object obj, Type declaredType, JsonSerializerOptions options);
    }

    public class MemberConverter<T, TM> : IMemberConverter
        where T : class
    {
        private readonly Func<T, TM> _memberGetter;
        private readonly Action<T, TM> _memberSetter;
        private readonly JsonConverter<TM> _jsonConverter;
        private readonly ReadOnlyMemory<byte> _memberName;
        private readonly TM _defaultValue;
        private readonly bool _ignoreIfDefault;
        private readonly Func<object, bool> _shouldSerializeMethod;
        private readonly RequirementPolicy _requirementPolicy;
        private readonly bool _isClass = typeof(TM).IsClass;

        public ReadOnlySpan<byte> MemberName => _memberName.Span;
        public string MemberNameAsString { get; }
        public bool IgnoreIfDefault => _ignoreIfDefault;
        public RequirementPolicy RequirementPolicy => _requirementPolicy;

        public MemberConverter(JsonSerializerOptions options, IMemberMapping memberMapping)
        {
            MemberNameAsString = memberMapping.MemberName;
            _memberName = Encoding.UTF8.GetBytes(MemberNameAsString);
            _memberGetter = GenerateGetter(memberMapping.MemberInfo);
            _memberSetter = GenerateSetter(memberMapping.MemberInfo);
            _jsonConverter = (JsonConverter<TM>)memberMapping.Converter;
            _defaultValue = (TM)memberMapping.DefaultValue;
            _ignoreIfDefault = memberMapping.IgnoreIfDefault;
            _shouldSerializeMethod = memberMapping.ShouldSerializeMethod;
            _requirementPolicy = memberMapping.RequirementPolicy;
        }

        public void Read(ref Utf8JsonReader reader, object obj, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                if (options.IgnoreNullValues)
                {
                    return;
                }

                if (_requirementPolicy == RequirementPolicy.DisallowNull || _requirementPolicy == RequirementPolicy.Always)
                {
                    throw new JsonException($"Property '{MemberNameAsString}' cannot be null.");
                }
            }

            _memberSetter((T)obj, _jsonConverter.Read(ref reader, typeof(TM), options));
        }

        public void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options)
        {
            TM value = _memberGetter((T)obj);

            if (_isClass && value == null && (_requirementPolicy == RequirementPolicy.DisallowNull
                || _requirementPolicy == RequirementPolicy.Always))
            {
                throw new JsonException($"Property '{MemberNameAsString}' cannot be null.");
            }

            _jsonConverter.Write(writer, _memberGetter((T)obj), options);
        }

        public object Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return _jsonConverter.Read(ref reader, typeof(TM), options);
        }

        public void Set(object obj, object value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                if (options.IgnoreNullValues)
                {
                    return;
                }
                if (_requirementPolicy == RequirementPolicy.DisallowNull || _requirementPolicy == RequirementPolicy.Always)
                {
                    throw new JsonException($"Required property '{MemberNameAsString}' cannot be null.");
                }
            }

            _memberSetter((T)obj, (TM)value);
        }

        public bool ShouldSerialize(object obj, Type declaredType, JsonSerializerOptions options)
        {
            if (options.IgnoreNullValues && typeof(TM).IsClass && _memberGetter((T)obj) == null)
            {
                return false;
            }

            if (IgnoreIfDefault && EqualityComparer<TM>.Default.Equals(_memberGetter((T)obj), _defaultValue))
            {
                return false;
            }

            if (_shouldSerializeMethod != null && !_shouldSerializeMethod(obj))
            {
                return false;
            }

            return true;
        }

        private Func<T, TM> GenerateGetter(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    if (propertyInfo.GetMethod.IsStatic)
                    {
                        if (!propertyInfo.CanRead)
                        {
                            return null;
                        }

                        ParameterExpression objParam = Expression.Parameter(typeof(T), "obj");
                        return Expression.Lambda<Func<T, TM>>(
                            Expression.Property(null, propertyInfo),
                            objParam).Compile();
                    }

                    return propertyInfo.CanRead
                       ? propertyInfo.GenerateGetter<T, TM>()
                       : null;

                case FieldInfo fieldInfo:
                    return fieldInfo.GenerateGetter<T, TM>();

                default:
                    return null;
            }
        }

        private Action<T, TM> GenerateSetter(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    if (!propertyInfo.CanWrite || propertyInfo.SetMethod.IsStatic)
                    {
                        return null;
                    }

                    return propertyInfo.GenerateSetter<T, TM>();

                case FieldInfo fieldInfo:
                    if (fieldInfo.IsStatic || fieldInfo.IsInitOnly)
                    {
                        return null;
                    }

                    return fieldInfo.GenerateSetter<T, TM>();

                default:
                    return null;
            }
        }
    }

    public class DiscriminatorMemberConverter<T> : IMemberConverter
        where T : class
    {
        private readonly IDiscriminatorConvention _discriminatorConvention;
        private readonly DiscriminatorPolicy _discriminatorPolicy;
        private readonly ReadOnlyMemory<byte> _memberName;

        public ReadOnlySpan<byte> MemberName => _memberName.Span;
        public string MemberNameAsString { get; private set; }
        public bool IgnoreIfDefault => false;
        public RequirementPolicy RequirementPolicy => RequirementPolicy.Never;

        public DiscriminatorMemberConverter(
            IDiscriminatorConvention discriminatorConvention,
            DiscriminatorPolicy discriminatorPolicy)
        {
            _discriminatorConvention = discriminatorConvention;
            _discriminatorPolicy = discriminatorPolicy;

            if (discriminatorConvention != null)
            {
                ReadOnlySpan<byte> discriminatorMemberName = discriminatorConvention.MemberName;
                _memberName = discriminatorMemberName.ToArray();
                MemberNameAsString = Encoding.UTF8.GetString(discriminatorMemberName);
            }
        }

        public void Read(ref Utf8JsonReader reader, object obj, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public object Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public void Set(object obj, object value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public bool ShouldSerialize(object obj, Type declaredType, JsonSerializerOptions options)
        {
            if (_discriminatorConvention == null)
            {
                return false;
            }

            DiscriminatorPolicy discriminatorPolicy = _discriminatorPolicy != DiscriminatorPolicy.Default ? _discriminatorPolicy
                : (options.GetDiscriminatorConventionRegistry().DiscriminatorPolicy != DiscriminatorPolicy.Default ? options.GetDiscriminatorConventionRegistry().DiscriminatorPolicy : DiscriminatorPolicy.Auto);

            return discriminatorPolicy == DiscriminatorPolicy.Always
                || discriminatorPolicy == DiscriminatorPolicy.Auto && obj.GetType() != declaredType;
        }

        public void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options)
        {
            _discriminatorConvention.WriteDiscriminator(writer, obj.GetType());
        }
    }
}
