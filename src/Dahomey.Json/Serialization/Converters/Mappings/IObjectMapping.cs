    using Dahomey.Json.Serialization.Conventions;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public interface IObjectMapping : IMappingInitialization
    {
        Type ObjectType { get; }
        JsonNamingPolicy PropertyNamingPolicy { get; }
        IReadOnlyCollection<IMemberMapping> MemberMappings { get; }
        ICreatorMapping CreatorMapping { get; }
        Delegate OnSerializingMethod { get; }
        Delegate OnSerializedMethod { get; }
        Delegate OnDeserializingMethod { get; }
        Delegate OnDeserializedMethod { get; }
        DiscriminatorPolicy DiscriminatorPolicy { get; }
        object Discriminator { get; }
        IExtensionDataMemberConverter ExtensionData { get; }
        bool IsDataContract { get; }

        void AutoMap();
        bool IsCreatorMember(ReadOnlySpan<byte> memberName);
    }
}
