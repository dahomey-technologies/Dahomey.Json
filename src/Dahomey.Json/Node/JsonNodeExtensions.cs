using Dahomey.Json.Util;

namespace System.Text.Json
{
    public static class JsonNodeExtensions
    {
        public static T ToObject<T>(this JsonNode node, JsonSerializerOptions? options = null)
        {
            using (ArrayBufferWriter<byte> bufferWriter = new ArrayBufferWriter<byte>())
            {
                using (var writer = new Utf8JsonWriter(bufferWriter))
                {
                    JsonSerializer.Serialize(writer, node, options);
                }

                return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
            }
        }
    }
}
