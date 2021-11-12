using System;

[assembly: CLSCompliant(true)]
namespace Dahomey.Json.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class JsonPropertyAttribute : Attribute
    {
    }
}
