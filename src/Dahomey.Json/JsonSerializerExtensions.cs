using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

[assembly: CLSCompliant(true)]
namespace Dahomey.Json
{
    public static class JsonSerializerExtensions
    {
        [return: MaybeNull]
        public static T DeserializeAnonymousType<T>(
            string json, T anonymousTypeObject, JsonSerializerOptions? options)
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}
