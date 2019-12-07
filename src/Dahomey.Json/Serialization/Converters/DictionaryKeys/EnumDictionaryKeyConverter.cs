using Dahomey.Json.Util;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.DictionaryKeys
{
    public class EnumDictionaryKeyConverter<T> : IDictionaryKeyConverter<T>
    {
        private readonly ByteBufferDictionary<T> _names2Values = new ByteBufferDictionary<T>();
        private readonly Dictionary<T, ReadOnlyMemory<byte>> _values2Names = new Dictionary<T, ReadOnlyMemory<byte>>();

        public EnumDictionaryKeyConverter()
        {
            string[] names = Enum.GetNames(typeof(T));
            T[] values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();

            for (int i = 0; i < names.Length; i++)
            {
                ReadOnlyMemory<byte> name = Encoding.ASCII.GetBytes(names[i]);
                T value = values[i];

                _names2Values.Add(name.Span, value);
                _values2Names.Add(value, name);
            }
        }

        public T Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            ReadOnlySpan<byte> name = reader.GetRawString();
            if (!_names2Values.TryGetValue(name, out T key))
            {
                throw new JsonException();
            }

            return key;
        }

        public string ToString(T key)
        {
            if (!_values2Names.TryGetValue(key, out ReadOnlyMemory<byte> name))
            {
                throw new JsonException();
            }

            return Encoding.ASCII.GetString(name.Span);
        }

        public void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (!_values2Names.TryGetValue(value, out ReadOnlyMemory<byte> name))
            {
                throw new JsonException();
            }

            writer.WritePropertyName(name.Span);
        }
    }
}
