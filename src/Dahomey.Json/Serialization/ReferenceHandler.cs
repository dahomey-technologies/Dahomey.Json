using System;
using System.Text.Json;
using System.Threading;

namespace Dahomey.Json.Serialization
{
    public struct ReferenceHandler : IDisposable
    {
        private readonly ReferenceContext _context;

        public ReferenceHandler(JsonSerializerOptions options)
        {
            AsyncLocal<ReferenceContext> currentReferenceContext = options.GetState().CurrentReferenceContext;
            ReferenceContext? context = currentReferenceContext.Value;
            if (context == null)
            {
                _context = new ReferenceContext();
                currentReferenceContext.Value = _context;
            }
            else
            {
                _context = context;
            }

            int maxDepth = options.MaxDepth == 0 ? 64 : options.MaxDepth;
            if (++_context.Depth > maxDepth)
            {
                throw new JsonException($"MaxDepth of {maxDepth} has been exceeded.");
            }
        }

        public bool IsReferenced(object obj)
        {
            return _context.IsReferenced(obj);
        }

        public void AddReference(object obj, string? reference = null)
        {
            _context.AddReference(obj, reference);
        }

        public string GetReference(object obj, out bool firstReference)
        {
            return _context.GetReference(obj, out firstReference);
        }

        public object? ResolveReference(string reference)
        {
            return _context.ResolveReference(reference);
        }

        public void Dispose()
        {
            if (--_context.Depth == 0)
            {
                _context.Reset();
            }
        }
    }
}
