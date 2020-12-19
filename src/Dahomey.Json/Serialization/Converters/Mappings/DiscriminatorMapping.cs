using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class DiscriminatorMapping<T> : IMemberMapping
    {
        private readonly DiscriminatorConventionRegistry _discriminatorConventionRegistry;
        private readonly IObjectMapping _objectMapping;

        public MemberInfo? MemberInfo => null;
        public Type MemberType => throw new NotSupportedException();
        public string? MemberName { get; set; }
        public JsonConverter? Converter { get; set; }
        public bool CanBeDeserialized => false;
        public bool CanBeSerialized => true;
        public object? DefaultValue { get; set; }
        public bool IgnoreIfDefault { get; set; }
        public Func<object, bool>? ShouldSerializeMethod { get; set; }
        public RequirementPolicy RequirementPolicy { get; set; } = RequirementPolicy.Never;

        public DiscriminatorMapping(DiscriminatorConventionRegistry discriminatorConventionRegistry, 
            IObjectMapping objectMapping)
        {
            _discriminatorConventionRegistry = discriminatorConventionRegistry;
            _objectMapping = objectMapping;
        }

        public void Initialize()
        {
        }

        public void PostInitialize()
        {
            IDiscriminatorConvention? discriminatorConvention = _discriminatorConventionRegistry.GetConvention(_objectMapping.ObjectType);
            if (discriminatorConvention != null)
            {
                MemberName = Encoding.UTF8.GetString(discriminatorConvention.MemberName);
            }
        }

        public IMemberConverter GenerateMemberConverter()
        {
            IDiscriminatorConvention? discriminatorConvention = _discriminatorConventionRegistry.GetConvention(_objectMapping.ObjectType);

            if (discriminatorConvention == null)
            {
                throw new JsonException($"Cannot find a discriminator convention for type {_objectMapping.ObjectType}");
            }

            IMemberConverter memberConverter = new DiscriminatorMemberConverter<T>(
                discriminatorConvention, _objectMapping.DiscriminatorPolicy);

            return memberConverter;
        }
    }
}
