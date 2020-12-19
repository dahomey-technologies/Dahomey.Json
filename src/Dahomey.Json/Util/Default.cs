using System;
using System.Linq;
using System.Text.Json;

namespace Dahomey.Json.Util
{
    public class Default
    {
        public static T Value<T>() => default!;
        public static object? Value(JsonSerializerOptions options,  Type type) => options.GetState().DefaultValues.GetOrAdd(type, GenerateValue);

        private static object? GenerateValue(Type type)
        {
            return typeof(Default)
                .GetMethods()
                .First(m => m.IsGenericMethod)
                .MakeGenericMethod(type)
                .Invoke(null, null);
        }
    }
}
