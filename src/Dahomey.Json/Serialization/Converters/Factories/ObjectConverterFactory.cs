using Dahomey.Json.Util;
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
                || typeToConvert.IsStruct()
                    && typeToConvert != typeof(DateTime)
                    && typeToConvert != typeof(DateTimeOffset)
                    && typeToConvert != typeof(Guid)
                    && typeToConvert != typeof(JsonElement)
                    && typeToConvert != typeof(Decimal);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type? nullableUnderlyingType = Nullable.GetUnderlyingType(typeToConvert);

            if (nullableUnderlyingType != null)
            {
                return CreateGenericConverter(options, typeof(NullableConverter<>), nullableUnderlyingType);
            }

            if (typeToConvert == typeof(object))
            {
                return new BaseObjectConverter();
            }

            return CreateGenericConverter(options, typeof(ObjectConverter<>), typeToConvert);
        }
    }
}
