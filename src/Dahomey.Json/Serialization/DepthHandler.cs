using System;
using System.Text.Json;

namespace Dahomey.Json.Serialization
{
    public struct DepthHandler : IDisposable
    {
        private readonly SerializationContext _context;

        public DepthHandler(JsonSerializerOptions options)
        {
            _context = SerializationContext.Current;
            int maxDepth = options.MaxDepth == 0 ? 64 : options.MaxDepth;
            if (++_context.Depth > maxDepth)
            {
                throw new JsonException($"MaxDepth of {maxDepth} has been exceeded.");
            }
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
