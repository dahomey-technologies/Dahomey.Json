using System;
using System.Collections.Generic;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public interface ICreatorMapping : IMappingInitialization
    {
        IReadOnlyCollection<ReadOnlyMemory<byte>>? MemberNames { get; }
        object CreateInstance(Dictionary<ReadOnlyMemory<byte>, object> values);
    }
}
