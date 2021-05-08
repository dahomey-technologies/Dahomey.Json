using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    internal sealed class UriConverter : JsonConverter<Uri>
    {
        public override Uri Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? uriString = reader.GetString();

            if (uriString == null)
            {
                return null!;
            }
            else
            {
                if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out Uri? value))
                {
                    return value;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value.OriginalString);
            }
        }
    }
}
