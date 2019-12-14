using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Factories
{
    public class JsonNodeConverterFactory : AbstractJsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(JsonNode).IsAssignableFrom(typeToConvert);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new JsonNodeConverter();
        }
    }
}
