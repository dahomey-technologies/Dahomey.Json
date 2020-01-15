namespace System.Text.Json
{
    public partial class JsonNumber
    {
        [CLSCompliant(false)]
        public static implicit operator sbyte(JsonNumber node)
        {
            return node.GetSByte();
        }

        public static implicit operator byte(JsonNumber node)
        {
            return node.GetByte();
        }

        public static implicit operator short(JsonNumber node)
        {
            return node.GetInt16();
        }

        [CLSCompliant(false)]
        public static implicit operator ushort(JsonNumber node)
        {
            return node.GetUInt16();
        }

        public static implicit operator int(JsonNumber node)
        {
            return node.GetInt32();
        }

        [CLSCompliant(false)]
        public static implicit operator uint(JsonNumber node)
        {
            return node.GetUInt32();
        }

        public static implicit operator long(JsonNumber node)
        {
            return node.GetInt64();
        }

        [CLSCompliant(false)]
        public static implicit operator ulong(JsonNumber node)
        {
            return node.GetUInt64();
        }

        public static implicit operator float(JsonNumber node)
        {
            return node.GetSingle();
        }

        public static implicit operator double(JsonNumber node)
        {
            return node.GetDouble();
        }

        public static implicit operator decimal(JsonNumber node)
        {
            return node.GetDecimal();
        }
    }
}
