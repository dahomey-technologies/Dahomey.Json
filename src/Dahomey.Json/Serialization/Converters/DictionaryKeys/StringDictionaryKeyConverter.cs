using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.DictionaryKeys
{
    public class StringDictionaryKeyConverter : IDictionaryKeyConverter<string>
    {
        public string Read(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            return reader.GetString();
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
