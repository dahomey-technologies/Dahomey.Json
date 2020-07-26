using System;

namespace Dahomey.Json.Attributes
{
#if NET5_0
    [AttributeUsage(AttributeTargets.Constructor)]
    public class JsonConstructorExAttribute : Attribute
    {
        public string[]? MemberNames { get; set; }

        public JsonConstructorExAttribute()
        {
        }

        public JsonConstructorExAttribute(params string[] memberNames)
        {
            MemberNames = memberNames;
        }
    }
#else
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class JsonConstructorAttribute : Attribute
    {
        public string[]? MemberNames { get; set; }

        public JsonConstructorAttribute()
        {
        }

        public JsonConstructorAttribute(params string[] memberNames)
        {
            MemberNames = memberNames;
        }
    }
#endif
}
