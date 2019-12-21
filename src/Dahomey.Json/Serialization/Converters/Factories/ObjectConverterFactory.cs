using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Factories
{
    public class ObjectConverterFactory : AbstractJsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsClass && typeToConvert != typeof(string)
                || typeToConvert.IsInterface
                || typeToConvert.IsValueType && !typeToConvert.IsPrimitive && !typeToConvert.IsEnum
                    && typeToConvert != typeof(DateTime)
                    && typeToConvert != typeof(DateTimeOffset)
                    && typeToConvert != typeof(Guid)
                    && typeToConvert != typeof(JsonElement);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return CreateGenericConverter(options, typeof(ObjectConverter<>), typeToConvert);
        }
    }
}
