using System;
using System.Threading;

namespace Dahomey.Json.Serialization
{
    public class SerializationContext
    {
        private readonly static AsyncLocal<SerializationContext> _current
            = new AsyncLocal<SerializationContext>();

        private Lazy<ReferenceResolver> _referenceResolver = new Lazy<ReferenceResolver>(() => new ReferenceResolver());

        public static SerializationContext Current
        {
            get
            {
                SerializationContext context = _current.Value;
                if (context == null)
                {
                    context = new SerializationContext();
                    _current.Value = context;
                }

                return context;
            }
        }

        public int Depth { get; set; }
        public ReferenceResolver ReferenceResolver => _referenceResolver.Value;

        public void Reset()
        {
            Depth = 0;
            if (_referenceResolver.IsValueCreated)
            {
                _referenceResolver.Value.Reset();
            }
        }
    }
}
