using Dahomey.Json.Util;
using System;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public class BaseObjectConverter : JsonConverter<object?>
    {
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                case JsonTokenType.StartArray:
                    return options.GetConverter<JsonNode>().Read(ref reader, typeToConvert, options);

                case JsonTokenType.String:
                    return options.GetConverter<string>().Read(ref reader, typeToConvert, options);

                case JsonTokenType.Number:
                    return ReadNumber(ref reader);

                case JsonTokenType.True:
                    return true;

                case JsonTokenType.False:
                    return false;

                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException();
            }
        }

        private object ReadNumber(ref Utf8JsonReader reader)
        {
            ReadOnlySpan<byte> buffer = reader.GetRawString();
            if (Utf8Parser.TryParse(buffer, out long lValue, out int bytesConsumed) && bytesConsumed == buffer.Length)
            {
                return lValue;
            }

            if (Utf8Parser.TryParse(buffer, out ulong ulValue, out bytesConsumed) && bytesConsumed == buffer.Length)
            {
                return ulValue;
            }

            if (Utf8Parser.TryParse(buffer, out double dblValue, out bytesConsumed) && bytesConsumed == buffer.Length)
            {
                return dblValue;
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            JsonConverter jsonConverter = options.GetConverter(value.GetType());
            jsonConverter.GetType().GetMethod("Write")!.Invoke(jsonConverter, new object[] { writer, value, options });
        }
    }
}
