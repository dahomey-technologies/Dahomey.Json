namespace System.Text.Json
{
    public partial class JsonBoolean
    {
        public static implicit operator bool(JsonBoolean node)
        {
            return node.Value;
        }
    }
}
