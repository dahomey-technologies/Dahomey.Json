using System;
using System.Collections.Concurrent;

namespace Dahomey.Json.Serialization
{
    public class ReferenceResolver
    {
        private readonly ConcurrentDictionary<string, object> _refs2Objects = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<object, string> _objects2Refs = new ConcurrentDictionary<object, string>();
        private int _nextReference = 1;

        public bool IsReferenced(object obj)
        {
            return _objects2Refs.ContainsKey(obj);
        }

        public void AddReference(object obj)
        {
            string reference = (_nextReference++).ToString();
            _refs2Objects.TryAdd(reference, obj);
            _objects2Refs.TryAdd(obj, reference);
        }

        public void Reset()
        {
            _objects2Refs.Clear();
            _refs2Objects.Clear();
            _nextReference = 1;
        }
    }
}
