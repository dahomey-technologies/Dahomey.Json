using System;

namespace Dahomey.Json.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class JsonNamingPolicyAttribute : Attribute
    {
        public JsonNamingPolicyAttribute(Type namingPolicyType)
        {
            NamingPolicyType = namingPolicyType;
        }

        public Type NamingPolicyType { get; set; }
    }
}
