using System;

namespace Dahomey.Json.Attributes
{
    /// <summary>
    /// Indicate whether a property/field is required.
    /// </summary>
    public enum RequirementPolicy
    {
        /// <summary>
        /// The property/field is not required. The default state.
        /// </summary>
        Never = 0,
        /// <summary>
        /// The property/field must be defined in JSON and cannot be a null value.
        /// </summary>
        Always = 1,
        /// <summary>
        /// The property/field must be defined in JSON but can be a null value.
        /// </summary>
        AllowNull = 2,
        /// <summary>
        /// The property/field is not required but it cannot be a null value.
        /// </summary>
        DisallowNull = 3
    }

    /// <summary>
    /// Indicate whether the underlying property or field is required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class JsonRequiredAttribute : Attribute
    {
        public RequirementPolicy Policy { get; set; }

        public JsonRequiredAttribute(RequirementPolicy policy = RequirementPolicy.Always)
        {
            Policy = policy;
        }
    }
}
