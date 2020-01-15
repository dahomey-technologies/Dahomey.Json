using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Dahomey.Json.Util
{
    public class Default
    {
        private static readonly ConcurrentDictionary<Type, object?> _values = new ConcurrentDictionary<Type, object?>();

        public static T Value<T>() => default!;
        public static object? Value(Type type) => _values.GetOrAdd(type, GenerateValue);

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
