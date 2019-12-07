using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.DictionaryKeys
{
    public interface IDictionaryKeyConverter<T>
    {
        string ToString(T key);
        T Read(ref Utf8JsonReader reader, JsonSerializerOptions options);
        void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options);
    }

}
