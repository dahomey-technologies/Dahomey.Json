using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Factories
{
    public abstract class AbstractJsonConverterFactory : JsonConverterFactory
    {
        protected JsonConverter CreateConverter(JsonSerializerOptions options, Type converterType)
        {
            JsonConverter? converter = (JsonConverter?)Activator.CreateInstance(converterType, options);

            if (converter == null)
            {
                throw new JsonException($"Cannot instantiate {converterType}");
            }

            return converter;
        }

        protected JsonConverter CreateGenericConverter(JsonSerializerOptions options, Type genericType, params Type[] typeArguments)
        {
            return CreateConverter(options, genericType.MakeGenericType(typeArguments));
        }
    }
}
