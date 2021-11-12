using System;

namespace Dahomey.Json.Attributes
{
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
}
