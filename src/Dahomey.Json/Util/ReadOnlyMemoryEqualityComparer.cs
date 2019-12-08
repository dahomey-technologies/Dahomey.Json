using System;
using System.Collections.Generic;
using System.Linq;

namespace Dahomey.Json.Util
{
    public class ReadOnlyMemoryEqualityComparer<T> : IEqualityComparer<ReadOnlyMemory<T>>
        where T : IEquatable<T>
    {
        public static ReadOnlyMemoryEqualityComparer<T> Instance { get; } = new ReadOnlyMemoryEqualityComparer<T>();

        public bool Equals(ReadOnlyMemory<T> x, ReadOnlyMemory<T> y)
        {
            return x.Span.SequenceEqual(y.Span);
        }

        public int GetHashCode(ReadOnlyMemory<T> obj)
        {
            int hash = 17;
            foreach (T t in obj.Span)
            {
                hash = 37 * hash + t.GetHashCode();
            }
            return hash;
        }
    }
}
