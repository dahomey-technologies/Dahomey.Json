using System;

namespace Dahomey.Json.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JsonIgnoreIfDefaultAttribute : Attribute
    {
    }
}
