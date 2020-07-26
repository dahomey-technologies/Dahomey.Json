using System;

namespace Dahomey.Json.Attributes
{
#if NETCOREAPP5_0
    [AttributeUsage(AttributeTargets.Constructor)]
    public class JsonConstructorExAttribute : Attribute
    {
        /// <summary>JSON serialized member names of the corresponding parameters of the decorated constructor.</summary>
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
        /// <summary>JSON serialized member names of the corresponding parameters of the decorated constructor.</summary>
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
