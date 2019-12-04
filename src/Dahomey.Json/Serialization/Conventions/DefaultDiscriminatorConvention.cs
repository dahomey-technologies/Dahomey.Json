using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Conventions
{
    public class DefaultDiscriminatorConvention : IDiscriminatorConvention
    {
        private readonly JsonSerializerOptions _options;
        private readonly ReadOnlyMemory<byte> _memberName;
        private readonly ConcurrentDictionary<string, Type> _typesByDiscriminator = new ConcurrentDictionary<string, Type>();
        private readonly ConcurrentDictionary<Type, string> _discriminatorsByType = new ConcurrentDictionary<Type, string>();

        public ReadOnlySpan<byte> MemberName => _memberName.Span;

        public DefaultDiscriminatorConvention(JsonSerializerOptions options)
            : this(options, "$type")
        {
        }

        public DefaultDiscriminatorConvention(JsonSerializerOptions options, string memberName)
        {
            _options = options;
            _memberName = Encoding.UTF8.GetBytes(memberName);
        }

        public bool TryRegisterType(Type type)
        {
            return true;
        }

        public Type ReadDiscriminator(ref Utf8JsonReader reader)
        {
            string discriminator = reader.GetString();
            Type type = _typesByDiscriminator.GetOrAdd(discriminator, NameToType);
            return type;
        }

        public void WriteDiscriminator(Utf8JsonWriter writer, Type actualType)
        {
            string discriminator = _discriminatorsByType.GetOrAdd(actualType, TypeToName);
            writer.WriteStringValue(discriminator);
        }

        private string TypeToName(Type type)
        {
            return type.FullName + ", " + type.Assembly.GetName().Name;
        }

        private Type NameToType(string name)
        {
            string[] parts = name.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string assemblyName;
            string typeName;

            switch(parts.Length)
            {
                case 1:
                    typeName = parts[0];
                    assemblyName = null;
                    break;

                case 2:
                    typeName = parts[0];
                    assemblyName = parts[1];
                    break;

                default:
                    throw new JsonException($"Invalid discriminator {name}");

            }

            if (!string.IsNullOrEmpty(assemblyName))
            {
                Assembly assembly = Assembly.Load(assemblyName);
                Type type = assembly.GetType(typeName);
                return type;
            }

            return Type.GetType(typeName);
        }
    }
}
