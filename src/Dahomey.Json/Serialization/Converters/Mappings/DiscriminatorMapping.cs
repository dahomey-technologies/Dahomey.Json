using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using System;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class DiscriminatorMapping<T> : IMemberMapping
    {
        private readonly DiscriminatorConventionRegistry _discriminatorConventionRegistry;
        private readonly IObjectMapping _objectMapping;

        public MemberInfo MemberInfo => null;
        public Type MemberType => null;
        public string MemberName { get; private set; }
        public JsonConverter Converter => null;
        public bool CanBeDeserialized => false;
        public bool CanBeSerialized => true;
        public object DefaultValue => null;
        public bool IgnoreIfDefault => false;
        public Func<object, bool> ShouldSerializeMethod => null;
        public RequirementPolicy RequirementPolicy => RequirementPolicy.Never;

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
            IDiscriminatorConvention discriminatorConvention = _discriminatorConventionRegistry.GetConvention(_objectMapping.ObjectType);
            if (discriminatorConvention != null)
            {
                MemberName = Encoding.UTF8.GetString(discriminatorConvention.MemberName);
            }
        }

        public IMemberConverter GenerateMemberConverter()
        {
            IDiscriminatorConvention discriminatorConvention = _discriminatorConventionRegistry.GetConvention(_objectMapping.ObjectType);

            IMemberConverter memberConverter = new DiscriminatorMemberConverter<T>(
                discriminatorConvention, _objectMapping.DiscriminatorPolicy);

            return memberConverter;
        }
    }
}
