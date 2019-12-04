using System;

namespace Dahomey.Json.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class JsonDiscriminatorAttribute : Attribute
    {
        public object Discriminator { get; set; }

        public JsonDiscriminatorAttribute(object discriminator)
        {
            Discriminator = discriminator;
        }
    }
}
