using System;
using System.Threading;

namespace Dahomey.Json.Serialization
{
    public class SerializationContext
    {
        private readonly static AsyncLocal<SerializationContext> _current
            = new AsyncLocal<SerializationContext>();

        private Lazy<ReferenceHandler> _referenceHandler = new Lazy<ReferenceHandler>(() => new ReferenceHandler());

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
        public ReferenceHandler ReferenceHandler => _referenceHandler.Value;

        public void Reset()
        {
            Depth = 0;
            if (_referenceHandler.IsValueCreated)
            {
                _referenceHandler.Value.Reset();
            }
        }
    }
}
