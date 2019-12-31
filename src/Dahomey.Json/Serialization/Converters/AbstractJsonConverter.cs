using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public abstract class AbstractJsonConverter<T> : JsonConverter<T>
    {
        public abstract void Read(ref Utf8JsonReader reader, ref T obj, JsonSerializerOptions options);
    }
}
