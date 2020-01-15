using System;
using System.Buffers;
using System.Buffers.Text;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.DictionaryKeys
{
    public class Utf8DictionaryKeyConverter<T> : IDictionaryKeyConverter<T>
    {
        private delegate bool TryParseDelegate(ReadOnlySpan<byte> source, out T value, out int bytesConsumed, char standardFormat);
        private delegate bool TryFormatDelegate(T value, Span<byte> destination, out int bytesWritten, StandardFormat format);

        private readonly TryParseDelegate _tryParseFunc;
        private readonly TryFormatDelegate _tryFormatFunc;

        public Utf8DictionaryKeyConverter()
        {
            MethodInfo? tryParseMethod = typeof(Utf8Parser).GetMethod(
                nameof(Utf8Parser.TryParse),
                new[] { typeof(ReadOnlySpan<byte>), typeof(T).MakeByRefType(), typeof(int).MakeByRefType(), typeof(char) });

            if (tryParseMethod == null)
            {
                throw new JsonException($"Cannot find method Utf8Parser.TryParse for type {typeof(T)}");
            }

            _tryParseFunc = (TryParseDelegate)tryParseMethod.CreateDelegate(typeof(TryParseDelegate));

            MethodInfo? tryFormatMethod = typeof(Utf8Formatter).GetMethod(
                nameof(Utf8Formatter.TryFormat),
                new[] { typeof(T), typeof(Span<byte>), typeof(int).MakeByRefType(), typeof(StandardFormat) });

            if (tryFormatMethod == null)
            {
                throw new JsonException($"Cannot find method Utf8Parser.TryFormat for type {typeof(T)}");
            }

            _tryFormatFunc = (TryFormatDelegate)tryFormatMethod.CreateDelegate(typeof(TryFormatDelegate));
        }

        public T Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            ReadOnlySpan<byte> rawString = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
            _tryParseFunc(rawString, out T value, out int bytesConsumed, '\0');
            return value;
        }

        public void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            Span<byte> rawString = stackalloc byte[1079];
            if (!_tryFormatFunc(value, rawString, out int bytesWritten, default))
            {
                throw new JsonException();
            }
            writer.WritePropertyName(rawString.Slice(0, bytesWritten));
        }

        public string ToString(T key)
        {
            Span<byte> rawString = stackalloc byte[1079];
            if (!_tryFormatFunc(key, rawString, out int bytesWritten, '\0'))
            {
                throw new JsonException();
            }
            return Encoding.ASCII.GetString(rawString.Slice(0, bytesWritten));
        }
    }

}
