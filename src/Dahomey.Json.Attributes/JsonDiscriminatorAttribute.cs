using System;

namespace Dahomey.Json.Attributes
{
    public enum DiscriminatorPolicy
    {
        Default,
        Auto,
        Never,
        Always
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class JsonDiscriminatorAttribute : Attribute
    {
        public object Discriminator { get; set; }

        public DiscriminatorPolicy Policy { get; set; }

        public JsonDiscriminatorAttribute(object discriminator)
        {
            Discriminator = discriminator;
        }
    }
}
