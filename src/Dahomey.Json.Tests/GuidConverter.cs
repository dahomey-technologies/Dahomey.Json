using Dahomey.Json.Util;
using System;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Tests
{
    public class GuidConverter : JsonConverter<Guid>
    {
        public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!Utf8Parser.TryParse(reader.GetRawString(), out Guid value, out int bytesConsumed, 'N'))
            {
                throw new JsonException();
            }
            return value;
        }

        public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
        {
            Span<byte> buffer = stackalloc byte[32];
            if (!Utf8Formatter.TryFormat(value, buffer, out int bytesWritten, 'N'))
            {
                throw new JsonException();
            }
            writer.WriteStringValue(buffer);
        }
    }
}
