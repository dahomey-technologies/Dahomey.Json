using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Dahomey.Json.Util
{
    public class ByteBufferDictionary<T>
    {
        [DebuggerDisplay("value={value}")]
        private class Node
        {
            public ulong key;
            public T value;
            public NodeCollection next;
        }

        private class NodeCollection
        {
            private Node[] _nodes;

            public void Add(Node node)
            {
                if (_nodes == null)
                {
                    _nodes = new[] { node };
                    return;
                }

                int index = BinarySearch(node.key);

                if (index >= 0)
                {
                    throw new InvalidOperationException("Duplicated key");
                }

                index = ~index;

                Node[] nodes = new Node[_nodes.Length + 1];

                if (index > 0)
                {
                    Array.Copy(_nodes, 0, nodes, 0, index);
                }

                if (index < _nodes.Length)
                {
                    Array.Copy(_nodes, index, nodes, index + 1, _nodes.Length - index);
                }

                nodes[index] = node;
                _nodes = nodes;
            }

            public bool TryGetValue(ulong key, out Node node)
            {
                if (_nodes == null)
                {
                    node = null;
                    return false;
                }

                int index = BinarySearch(key);

                if (index < 0)
                {
                    node = null;
                    return false;
                }

                node = _nodes[index];
                return true;
            }

            private int BinarySearch(ulong key)
            {
                int lo = 0;
                int hi = _nodes.Length - 1;

                ref Node start = ref Unsafe.AsRef(_nodes[0]);

                while (lo <= hi)
                {
                    int i = lo + (hi - lo) / 2;
                    int order = Unsafe.Add(ref start, i).key.CompareTo(key);

                    if (order == 0)
                    {
                        return i;
                    }

                    if (order < 0)
                    {
                        lo = i + 1;
                    }
                    else
                    {
                        hi = i - 1;
                    }
                }

                return ~lo;
            }
        }

        private readonly Node _root = new Node
        {
            next = new NodeCollection()
        };

        public void Add(ReadOnlySpan<byte> key, T value)
        {
            int len = key.Length;

            Span<ulong> segments = stackalloc ulong[(len + 7) / 8];
            key.CopyTo(MemoryMarshal.AsBytes(segments));

            Node last = _root;

            foreach (ulong segment in segments)
            {
                Node node;

                if (last.next == null)
                {
                    last.next = new NodeCollection();
                    last.next.Add(node = new Node { key = segment });
                }
                else if (!last.next.TryGetValue(segment, out node))
                {
                    last.next.Add(node = new Node { key = segment });
                }

                last = node;
            }

            last.value = value;
        }

        public bool TryGetValue(ReadOnlySpan<byte> key, out T value)
        {
            Node node;
            int len = key.Length;

            if (len == 0)
            {
                value = default;
                return false;
            }

            if (len <= 8)
            {
                ulong singleKey = 0;

                ref byte dstRef = ref Unsafe.As<ulong, byte>(ref singleKey);
                ref byte srcRef = ref MemoryMarshal.GetReference(key);

                Unsafe.CopyBlock(ref dstRef, ref srcRef, (uint)len);

                if (_root.next == null || !_root.next.TryGetValue(singleKey, out node))
                {
                    value = default;
                    return false;
                }
            }
            else
            {
                Span<ulong> segments = stackalloc ulong[(len + 7) / 8];
                key.CopyTo(MemoryMarshal.AsBytes(segments));

                node = _root;
                foreach (ulong segment in segments)
                {
                    if (node.next == null || !node.next.TryGetValue(segment, out node))
                    {
                        value = default;
                        return false;
                    }
                }
            }

            value = node.value;
            return true;
        }
    }
}
