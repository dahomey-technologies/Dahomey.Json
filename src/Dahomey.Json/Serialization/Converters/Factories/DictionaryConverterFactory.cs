using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Factories
{
    public class DictionaryConverterFactory : AbstractJsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType
                && (typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableDictionary<,>)
                || typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableSortedDictionary<,>)
                || typeToConvert.GetInterfaces()
                    .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                || typeToConvert.IsInterface
                    && (typeToConvert.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                    || typeToConvert.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)));
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert.IsGenericType)
            {
                if (typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableDictionary<,>)
                    || typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableSortedDictionary<,>))
                {
                    Type keyType = typeToConvert.GetGenericArguments()[0];
                    Type valueType = typeToConvert.GetGenericArguments()[1];

                    return CreateGenericConverter(
                        options,
                        typeof(ImmutableDictionaryConverter<,,>), typeToConvert, keyType, valueType);
                }

                if (typeToConvert.GetInterfaces()
                    .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                {
                    Type keyType = typeToConvert.GetGenericArguments()[0];
                    Type valueType = typeToConvert.GetGenericArguments()[1];
                    return CreateGenericConverter(
                        options,
                        typeof(DictionaryConverter<,,>), typeToConvert, keyType, valueType);
                }

                if (typeToConvert.IsInterface
                    && typeToConvert.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
                {
                    Type keyType = typeToConvert.GetGenericArguments()[0];
                    Type valueType = typeToConvert.GetGenericArguments()[1];
                    return CreateGenericConverter(
                        options,
                        typeof(InterfaceDictionaryConverter<,,>),
                        typeToConvert, keyType, valueType);
                }

                if (typeToConvert.IsInterface
                    && typeToConvert.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    Type keyType = typeToConvert.GetGenericArguments()[0];
                    Type valueType = typeToConvert.GetGenericArguments()[1];
                    return CreateGenericConverter(
                        options,
                        typeof(InterfaceDictionaryConverter<,,>),
                        typeToConvert, keyType, valueType);
                }
            }

            throw new JsonException();
        }
    }
}
