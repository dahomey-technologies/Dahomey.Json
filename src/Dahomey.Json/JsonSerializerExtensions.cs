using System.Text.Json;

namespace Dahomey.Json
{
    public static class JsonSerializerExtensions
    {
        public static T DeserializeAnonymousType<T>(
            string json, T anonymousTypeObject, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}
