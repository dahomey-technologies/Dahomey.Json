using System;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class JsonDeserializeAttribute : JsonAttribute
    {
    }
}
