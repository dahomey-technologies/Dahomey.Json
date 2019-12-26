namespace System.Text
{
    internal static class EncodingExtensions
    {
        public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
        {
            fixed (byte* bytesPtr = bytes)
            {
                return bytesPtr == null ? string.Empty : encoding.GetString(bytesPtr, bytes.Length);
            }
        }

        public static unsafe int GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            fixed (char* charsPtr = chars)
            fixed (byte* bytesPtr = bytes)
            {
                return encoding.GetBytes(charsPtr, chars.Length, bytesPtr, bytes.Length);
            }
        }
    }
}
