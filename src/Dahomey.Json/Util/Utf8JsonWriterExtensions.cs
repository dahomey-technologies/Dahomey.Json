using System;
using System.Text.Json;

namespace Dahomey.Json.Util
{
    public static class Utf8JsonWriterExtensions
    {
        internal delegate void WriteFormatedNumberValueFuncDelegate(Utf8JsonWriter writer, ReadOnlySpan<byte> utf8FormattedNumber);

        public static void WriteNumberValue(this Utf8JsonWriter writer, JsonSerializerOptions options, ReadOnlySpan<byte> utf8FormattedNumber)
        {
            options.GetState().WriteFormatedNmberValueFunc(writer, utf8FormattedNumber);
        }
    }
}
