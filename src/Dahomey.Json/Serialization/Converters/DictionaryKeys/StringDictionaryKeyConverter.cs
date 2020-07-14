using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.DictionaryKeys
{
    public class StringDictionaryKeyConverter : IDictionaryKeyConverter<string>
    {
        public string Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            string? key = reader.GetString();

            if (key == null)
            {
                throw new JsonException("Dictionary key cannot be null");
            }

            return key;
        }

        public void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WritePropertyName(value);
        }

        public string ToString(string key)
        {
            return key;
        }
    }

}
