using System;
using System.Collections.Generic;
using System.Text;

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
