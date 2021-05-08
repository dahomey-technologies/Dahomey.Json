using System;
using System.Collections.Generic;

namespace Dahomey.Json.Util
{
    public static class DictionaryExtensions
    {
#if NETSTANDARD2_0
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }
#endif
    }
}
