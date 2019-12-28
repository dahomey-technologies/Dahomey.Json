using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public class NullableConverter<T> : JsonConverter<T?> where T : struct
    {
        private readonly JsonConverter<T> _jsonConverter;

        public NullableConverter(JsonSerializerOptions options)
        {
            _jsonConverter = options.GetConverter<T>();
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            return _jsonConverter.Read(ref reader, typeToConvert, options);
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            _jsonConverter.Write(writer, value.Value, options);
        }
    }
}
