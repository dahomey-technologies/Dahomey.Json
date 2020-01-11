namespace System.Text.Json
{
    public partial class JsonString
    {
        public static implicit operator string(JsonString node)
        {
            return node.Value;
        }
    }
}
