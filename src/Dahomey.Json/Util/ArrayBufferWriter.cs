using System;
using System.Buffers;
using System.Diagnostics;

namespace Dahomey.Json.Util
{
    public class ArrayBufferWriter<T> : IBufferWriter<T>, IDisposable
    {
        private readonly T[] _emptyBuffer = new T[0];

        private T[] _buffer;
        private int _size;

        private const int DefaultInitialBufferSize = 256;

        public int Capacity => _buffer.Length;
        public int FreeCapacity => _buffer.Length - _size;
        public ReadOnlySpan<T> WrittenSpan => _buffer.AsSpan(0, _size);

        public ArrayBufferWriter()
        {
            _buffer = _emptyBuffer;
            _size = 0;
        }

        public void Dispose()
        {
            if (!ReferenceEquals(_buffer, _emptyBuffer))
            {
                ArrayPool<T>.Shared.Return(_buffer);
            }

            _buffer = null;
            _size = 0;
        }

        public void Advance(int count)
        {
            if (count < 0)
                throw new ArgumentException(nameof(count));

            if (_size > _buffer.Length - count)
                throw new InvalidOperationException();

            _size += count;
        }

        public Memory<T> GetMemory(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            Debug.Assert(_buffer.Length > _size);
            return _buffer.AsMemory(_size);
        }

        public Span<T> GetSpan(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            Debug.Assert(_buffer.Length > _size);
            return _buffer.AsSpan(_size);
        }

        private void CheckAndResizeBuffer(int sizeHint)
        {
            if (sizeHint < 0)
                throw new ArgumentException(nameof(sizeHint));

            if (sizeHint == 0)
            {
                sizeHint = 1;
            }

            if (sizeHint > FreeCapacity)
            {
                int growBy = Math.Max(sizeHint, _buffer.Length);

                if (_buffer.Length == 0)
                {
                    growBy = Math.Max(growBy, DefaultInitialBufferSize);
                }

                int newSize = checked(_buffer.Length + growBy);

                T[] backup = _buffer;
                _buffer = ArrayPool<T>.Shared.Rent(newSize);

                if (!ReferenceEquals(backup, _emptyBuffer))
                {
                    backup.AsSpan().CopyTo(_buffer);
                    ArrayPool<T>.Shared.Return(backup);
                }
            }

            Debug.Assert(FreeCapacity > 0 && FreeCapacity >= sizeHint);
        }
    }
}
