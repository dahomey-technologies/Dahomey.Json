using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Factories
{
    public abstract class AbstractJsonConverterFactory : JsonConverterFactory
    {
        protected JsonConverter CreateConverter(JsonSerializerOptions options, Type converterType)
        {
            return (JsonConverter)Activator.CreateInstance(converterType, options);
        }

        protected JsonConverter CreateGenericConverter(JsonSerializerOptions options, Type genericType, params Type[] typeArguments)
        {
            return CreateConverter(options, genericType.MakeGenericType(typeArguments));
        }
    }
}
