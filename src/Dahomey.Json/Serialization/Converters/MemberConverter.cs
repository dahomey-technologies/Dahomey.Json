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
        string? MemberNameAsString { get; }
        RequirementPolicy RequirementPolicy { get; }

        void Read(ref Utf8JsonReader reader, object obj, JsonSerializerOptions options);
        void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options);
        object Read(ref Utf8JsonReader reader, JsonSerializerOptions options);
        void Set(object obj, object value, JsonSerializerOptions options);
        bool ShouldSerialize(object obj, Type declaredType, JsonSerializerOptions options);
    }

    public interface IMemberConverter<T>
    {
        void Read(ref Utf8JsonReader reader, ref T instance, JsonSerializerOptions options);
        void Write(Utf8JsonWriter writer, ref T instance, JsonSerializerOptions options);
        bool ShouldSerialize(ref T instance, Type declaredType, JsonSerializerOptions options);
    }

    public class MemberConverter<T, TM> : IMemberConverter
        where T : class
    {
        private readonly Func<T, TM>? _memberGetter;
        private readonly Action<T, TM>? _memberSetter;
        private readonly JsonConverter<TM> _jsonConverter;
        private readonly ReadOnlyMemory<byte> _memberName;
        private readonly TM _defaultValue;
        private readonly bool _ignoreIfDefault;
        private readonly Func<object, bool>? _shouldSerializeMethod;
        private readonly RequirementPolicy _requirementPolicy;
        private readonly bool _isClass = typeof(TM).IsClass;
        private readonly bool _canBeNull = typeof(TM).IsClass || Nullable.GetUnderlyingType(typeof(TM)) != null;
        private readonly bool _deserializableReadOnlyProperty;

        public ReadOnlySpan<byte> MemberName => _memberName.Span;
        public string MemberNameAsString { get; }
        public bool IgnoreIfDefault => _ignoreIfDefault;
        public RequirementPolicy RequirementPolicy => _requirementPolicy;

        public MemberConverter(JsonSerializerOptions options, IMemberMapping memberMapping)
        {
            MemberInfo? memberInfo = memberMapping.MemberInfo;

            if (memberInfo == null)
            {
                throw new JsonException("MemberInfo must not be null");
            }

            MemberNameAsString = memberMapping.MemberName!;
            _memberName = Encoding.UTF8.GetBytes(MemberNameAsString);
            _memberGetter = GenerateGetter(memberInfo);
            _memberSetter = GenerateSetter(memberInfo);
            _jsonConverter = (JsonConverter<TM>)memberMapping.Converter!;
            _defaultValue = (TM)memberMapping.DefaultValue!;
            _ignoreIfDefault = memberMapping.IgnoreIfDefault;
            _shouldSerializeMethod = memberMapping.ShouldSerializeMethod;
            _requirementPolicy = memberMapping.RequirementPolicy;
            _deserializableReadOnlyProperty = options.GetReadOnlyPropertyHandling() == ReadOnlyPropertyHandling.Read
                || (memberInfo.IsDefined(typeof(JsonDeserializeAttribute)) && options.GetReadOnlyPropertyHandling() == ReadOnlyPropertyHandling.Default);
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

            if (_deserializableReadOnlyProperty && _memberSetter == null)
            {
                if (_memberGetter == null)
                {
                    throw new JsonException($"No member getter for '{MemberNameAsString}'");
                }

                TM value = _memberGetter((T)obj);

                if (value == null)
                {
                    throw new JsonException($"Property '{MemberNameAsString}' decorated by JsonDeserializeAttribute should be instantiated");
                }

                ((AbstractJsonConverter<TM>)_jsonConverter).Read(ref reader, ref value, options);
            }
            else
            {
                if (_memberSetter == null)
                {
                    throw new JsonException($"No member setter for '{MemberNameAsString}'");
                }

                try
                {
                    _memberSetter((T)obj, _jsonConverter.Read(ref reader, typeof(TM), options)!);
                }
                catch(Exception ex)
                {
                    throw new MemberJsonException(MemberNameAsString, typeof(TM), ex);
                }
            }
        }

        public void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options)
        {
            if (_memberGetter == null)
            {
                throw new JsonException($"No member getter for '{MemberNameAsString}'");
            }

            TM value = _memberGetter((T)obj);

            if (_isClass && value == null && (_requirementPolicy == RequirementPolicy.DisallowNull
                || _requirementPolicy == RequirementPolicy.Always))
            {
                throw new JsonException($"Property '{MemberNameAsString}' cannot be null.");
            }

            _jsonConverter.Write(writer, value, options);
        }

        public object Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            try
            {
                return _jsonConverter.Read(ref reader, typeof(TM), options)!;
            }
            catch (Exception ex)
            {
                throw new MemberJsonException(MemberNameAsString, typeof(TM), ex);
            }
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

            if (_memberSetter == null)
            {
                throw new JsonException($"No member setter for '{MemberNameAsString}'");
            }

            _memberSetter((T)obj, (TM)value!);
        }

        public bool ShouldSerialize(object obj, Type declaredType, JsonSerializerOptions options)
        {
            if (_memberGetter == null)
            {
                throw new JsonException($"No member getter for '{MemberNameAsString}'");
            }

            if (options.IgnoreNullValues && _canBeNull && _memberGetter((T)obj) == null)
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

        private Func<T, TM>? GenerateGetter(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsStatic)
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

        private Action<T, TM>? GenerateSetter(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    if (!propertyInfo.CanWrite || (propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsStatic))
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

    public class StructMemberConverter<T, TM> : IMemberConverter, IMemberConverter<T>
        where T : struct
    {
        private readonly StructMemberGetterDelegate<T, TM>? _memberGetter;
        private readonly StructMemberSetterDelegate<T, TM>? _memberSetter;
        private readonly JsonConverter<TM> _jsonConverter;
        private readonly ReadOnlyMemory<byte> _memberName;
        private readonly TM _defaultValue;
        private readonly bool _ignoreIfDefault;
        private readonly RequirementPolicy _requirementPolicy;
        private readonly bool _isClass = typeof(TM).IsClass;
        private readonly bool _canBeNull = typeof(TM).IsClass || Nullable.GetUnderlyingType(typeof(TM)) != null;

        public ReadOnlySpan<byte> MemberName => _memberName.Span;
        public string MemberNameAsString { get; }
        public bool IgnoreIfDefault => _ignoreIfDefault;
        public RequirementPolicy RequirementPolicy => _requirementPolicy;

        public StructMemberConverter(JsonSerializerOptions options, IMemberMapping memberMapping)
        {
            MemberInfo? memberInfo = memberMapping.MemberInfo;

            if (memberInfo == null)
            {
                throw new JsonException("MemberInfo must not be null");
            }

            MemberNameAsString = memberMapping.MemberName!;
            _memberName = Encoding.UTF8.GetBytes(MemberNameAsString);
            _memberGetter = GenerateGetter(memberInfo);
            _memberSetter = GenerateSetter(memberInfo);
            _jsonConverter = (JsonConverter<TM>)memberMapping.Converter!;
            _defaultValue = (TM)memberMapping.DefaultValue!;
            _ignoreIfDefault = memberMapping.IgnoreIfDefault;
            _requirementPolicy = memberMapping.RequirementPolicy;
        }

        public void Read(ref Utf8JsonReader reader, object obj, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public object Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            try
            {
                return _jsonConverter.Read(ref reader, typeof(TM), options)!;
            }
            catch (Exception ex)
            {
                throw new MemberJsonException(MemberNameAsString, typeof(TM), ex);
            }
        }

        public void Set(object obj, object value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public bool ShouldSerialize(object obj, Type declaredType, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public void Read(ref Utf8JsonReader reader, ref T instance, JsonSerializerOptions options)
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

            if (_memberSetter == null)
            {
                throw new JsonException($"No member setter for '{MemberNameAsString}'");
            }

            try
            {
                _memberSetter(ref instance, _jsonConverter.Read(ref reader, typeof(TM), options)!);
            }
            catch (Exception ex)
            {
                throw new MemberJsonException(MemberNameAsString, typeof(TM), ex);
            }
        }

        public void Write(Utf8JsonWriter writer, ref T instance, JsonSerializerOptions options)
        {
            if (_memberGetter == null)
            {
                throw new JsonException($"No member getter for '{MemberNameAsString}'");
            }

            TM value = _memberGetter(ref instance);

            if (_isClass && value == null && (_requirementPolicy == RequirementPolicy.DisallowNull
                || _requirementPolicy == RequirementPolicy.Always))
            {
                throw new JsonException($"Property '{MemberNameAsString}' cannot be null.");
            }

            _jsonConverter.Write(writer, value, options);
        }

        public bool ShouldSerialize(ref T instance, Type declaredType, JsonSerializerOptions options)
        {
            if (_memberGetter == null)
            {
                throw new JsonException($"No member getter for '{MemberNameAsString}'");
            }

            if (options.IgnoreNullValues && _canBeNull && _memberGetter(ref instance) == null)
            {
                return false;
            }

            if (IgnoreIfDefault && EqualityComparer<TM>.Default.Equals(_memberGetter(ref instance), _defaultValue))
            {
                return false;
            }

            return true;
        }

        private StructMemberGetterDelegate<T, TM>? GenerateGetter(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsStatic)
                    {
                        if (!propertyInfo.CanRead)
                        {
                            return null;
                        }

                        ParameterExpression instanceParam = Expression.Parameter(typeof(T), "instance");
                        return Expression.Lambda<StructMemberGetterDelegate<T, TM>>(
                            Expression.Property(null, propertyInfo),
                            instanceParam).Compile();
                    }

                    return propertyInfo.CanRead
                       ? propertyInfo.GenerateStructGetter<T, TM>()
                       : null;

                case FieldInfo fieldInfo:
                    return fieldInfo.GenerateStructGetter<T, TM>();

                default:
                    return null;
            }
        }

        private StructMemberSetterDelegate<T, TM>? GenerateSetter(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    if (!propertyInfo.CanWrite || (propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsStatic))
                    {
                        return null;
                    }

                    return propertyInfo.GenerateStructSetter<T, TM>();

                case FieldInfo fieldInfo:
                    if (fieldInfo.IsStatic || fieldInfo.IsInitOnly)
                    {
                        return null;
                    }

                    return fieldInfo.GenerateStructSetter<T, TM>();

                default:
                    return null;
            }
        }
    }

    public class DiscriminatorMemberConverter<T> : IMemberConverter
    {
        private readonly IDiscriminatorConvention _discriminatorConvention;
        private readonly DiscriminatorPolicy _discriminatorPolicy;
        private readonly ReadOnlyMemory<byte> _memberName;

        public ReadOnlySpan<byte> MemberName => _memberName.Span;
        public string? MemberNameAsString { get; private set; }
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
