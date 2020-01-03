using System.Collections.Concurrent;
using System.Text;

namespace Dahomey.Json.Serialization
{
    public class ReferenceHandler
    {
        public static readonly byte[] ID_MEMBER_NAME = Encoding.ASCII.GetBytes("$id");
        public static readonly byte[] REF_MEMBER_NAME = Encoding.ASCII.GetBytes("$ref");
        public static readonly byte[] VALUES_MEMBER_NAME = Encoding.ASCII.GetBytes("$values");

        private readonly ConcurrentDictionary<string, object> _refs2Objects = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<object, string> _objects2Refs = new ConcurrentDictionary<object, string>();
        private int _nextReference = 1;

        public bool IsReferenced(object obj)
        {
            return _objects2Refs.ContainsKey(obj);
        }

        public void AddReference(object obj, string reference = null)
        {
            if (reference == null)
            {
                reference = (_nextReference++).ToString();
            }
            _refs2Objects.TryAdd(reference, obj);
            _objects2Refs.TryAdd(obj, reference);
        }

        public string GetReference(object obj, out bool firstReference)
        {
            bool firstRef = false;
            string @ref =  _objects2Refs.GetOrAdd(obj, newObj =>
            {
                string reference = (_nextReference++).ToString();
                _refs2Objects.TryAdd(reference, newObj);
                firstRef = true;
                return reference;
            });

            firstReference = firstRef;
            return @ref;
        }

        public object ResolveReference(string @ref)
        {
            _refs2Objects.TryGetValue(@ref, out object @object);
            return @object;
        }

        public void Reset()
        {
            _objects2Refs.Clear();
            _refs2Objects.Clear();
            _nextReference = 1;
        }
    }
}
