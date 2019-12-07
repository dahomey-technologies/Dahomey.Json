using System;
using System.Buffers;
using System.Text.Json;

namespace Dahomey.Json.Util
{
    public static class Utf8JsonReaderExtensions
    {
        public static ReadOnlySpan<byte> GetRawString(this Utf8JsonReader reader)
        {
            return reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
        }
    }
}
