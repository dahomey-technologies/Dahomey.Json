using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Factories
{
    public class CollectionConverterFactory : AbstractJsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsArray && typeToConvert != typeof(byte[])
                || (typeToConvert.IsGenericType
                && (typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableArray<>)
                || typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableList<>)
                || typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableSortedSet<>)
                || typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableHashSet<>)
                || typeToConvert.GetInterfaces()
                    .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>))
                || typeToConvert.IsInterface
                    && (typeToConvert.GetGenericTypeDefinition() == typeof(ISet<>)
                        || typeToConvert.GetGenericTypeDefinition() == typeof(IList<>)
                        || typeToConvert.GetGenericTypeDefinition() == typeof(ICollection<>)
                        || typeToConvert.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                        || typeToConvert.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
                        || typeToConvert.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))));
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert.IsArray)
            {
                Type? itemType = typeToConvert.GetElementType();

                if (itemType == null)
                {
                    throw new JsonException("Unexpected");
                }

                return CreateGenericConverter(
                    options,
                    typeof(ArrayConverter<>), itemType);
            }

            if (typeToConvert.IsGenericType)
            {
                if (typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableArray<>)
                    || typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableList<>)
                    || typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableSortedSet<>)
                    || typeToConvert.GetGenericTypeDefinition() == typeof(ImmutableHashSet<>))
                {
                    Type itemType = typeToConvert.GetGenericArguments()[0];
                    return CreateGenericConverter(
                        options,
                        typeof(ImmutableCollectionConverter<,>), typeToConvert, itemType);
                }

                if (typeToConvert.IsInterface
                    && typeToConvert.GetGenericTypeDefinition() == typeof(ISet<>))
                {
                    Type itemType = typeToConvert.GetGenericArguments()[0];
                    return CreateGenericConverter(
                        options,
                        typeof(InterfaceCollectionConverter<,,>),
                        typeof(HashSet<>).MakeGenericType(itemType), typeToConvert, itemType);
                }

                if (typeToConvert.IsInterface
                    && (typeToConvert.GetGenericTypeDefinition() == typeof(IList<>)
                    || typeToConvert.GetGenericTypeDefinition() == typeof(ICollection<>)
                    || typeToConvert.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    || typeToConvert.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
                    || typeToConvert.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>)))
                {
                    Type itemType = typeToConvert.GetGenericArguments()[0];
                    return CreateGenericConverter(
                        options,
                        typeof(InterfaceCollectionConverter<,,>),
                        typeof(List<>).MakeGenericType(itemType), typeToConvert, itemType);
                }

                if (typeToConvert.GetInterfaces()
                    .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>)))
                {
                    Type itemType = typeToConvert.GetGenericArguments()[0];
                    return CreateGenericConverter(
                        options,
                        typeof(CollectionConverter<,>), typeToConvert, itemType);
                }
            }

            throw new JsonException();
        }
    }
}
