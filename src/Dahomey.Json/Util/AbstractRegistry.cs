using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Util
{
    public class AbstractRegistry<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
