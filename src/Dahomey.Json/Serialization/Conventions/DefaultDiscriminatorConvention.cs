using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using System.Text;
using Dahomey.Json.Serialization.Converters.Mappings;

namespace Dahomey.Json.Serialization.Conventions
{
    public class DefaultDiscriminatorConvention<T> : IDiscriminatorConvention
        where T : notnull
    {
        private readonly JsonSerializerOptions _options;
        private readonly ReadOnlyMemory<byte> _memberName;
        private readonly Dictionary<T, Type> _typesByDiscriminator = new Dictionary<T, Type>();
        private readonly Dictionary<Type, T> _discriminatorsByType = new Dictionary<Type, T>();
        private readonly JsonConverter<T> _jsonConverter;

        public ReadOnlySpan<byte> MemberName => _memberName.Span;

        public DefaultDiscriminatorConvention(JsonSerializerOptions options)
            : this(options, "$type")
        {
        }

        public DefaultDiscriminatorConvention(JsonSerializerOptions options, string memberName)
        {
            _options = options;
            _memberName = Encoding.UTF8.GetBytes(memberName);
            _jsonConverter = options.GetConverter<T>();
        }

        public bool TryRegisterType(Type type)
        {
            IObjectMapping objectMapping = _options.GetObjectMappingRegistry().Lookup(type);

            if (objectMapping.Discriminator == null || !(objectMapping.Discriminator is T discriminator))
            {
                return false;
            }

            _discriminatorsByType[type] = discriminator;
            _typesByDiscriminator.Add(discriminator, type);
            return true;
        }

        public Type ReadDiscriminator(ref Utf8JsonReader reader)
        {
            T discriminator = _jsonConverter.Read(ref reader, typeof(T), _options);

            if (discriminator == null)
            {
                throw new JsonException($"Null discriminator");
            }

            if (!_typesByDiscriminator.TryGetValue(discriminator, out Type? type))
            {
                throw new JsonException($"Unknown type discriminator: {discriminator}");
            }
            return type;
        }

        public void WriteDiscriminator(Utf8JsonWriter writer, Type actualType)
        {
            if (!_discriminatorsByType.TryGetValue(actualType, out T discriminator))
            {
                throw new JsonException($"Unknown discriminator for type: {actualType}");
            }

            _jsonConverter.Write(writer, discriminator, _options);
        }
    }
}
