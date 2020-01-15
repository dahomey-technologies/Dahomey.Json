using System;
using System.Reflection;
using System.Text.Json;

namespace Dahomey.Json.Util
{
    public static class Utf8JsonWriterExtensions
    {
        private delegate void WriteFormatedNumberValueFuncDelegate(Utf8JsonWriter writer, ReadOnlySpan<byte> utf8FormattedNumber);
        private static readonly WriteFormatedNumberValueFuncDelegate s_writeFormatedNmberValueFunc;

        static Utf8JsonWriterExtensions()
        {
            MethodInfo? method = typeof(Utf8JsonWriter).GetMethod(
                nameof(Utf8JsonWriter.WriteNumberValue),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(ReadOnlySpan<byte>) },
                null);

            if (method == null)
            {
                throw new JsonException("Unexpected");
            }

            s_writeFormatedNmberValueFunc = 
                (WriteFormatedNumberValueFuncDelegate)method
                    .CreateDelegate(typeof(WriteFormatedNumberValueFuncDelegate));
        }

        public static void WriteNumberValue(this Utf8JsonWriter writer, ReadOnlySpan<byte> utf8FormattedNumber)
        {
            s_writeFormatedNmberValueFunc(writer, utf8FormattedNumber);
        }
    }
}
