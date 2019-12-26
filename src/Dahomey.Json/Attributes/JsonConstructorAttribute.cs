using System;

namespace Dahomey.Json.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public class JsonConstructorAttribute : Attribute
    {
        public string[] MemberNames { get; set; }

        public JsonConstructorAttribute()
        {
        }

        public JsonConstructorAttribute(params string[] memberNames)
        {
            MemberNames = memberNames;
        }
    }
}
