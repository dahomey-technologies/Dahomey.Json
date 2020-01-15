using Dahomey.Json.Attributes;
using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public interface IMemberMapping : IMappingInitialization
    {
        MemberInfo? MemberInfo { get; }
        Type MemberType { get; }
        string? MemberName { get; }
        JsonConverter? Converter { get; }
        bool CanBeDeserialized { get; }
        bool CanBeSerialized { get; }
        object? DefaultValue { get; }
        bool IgnoreIfDefault { get; }
        Func<object, bool>? ShouldSerializeMethod { get; }
        RequirementPolicy RequirementPolicy { get; }
        IMemberConverter GenerateMemberConverter();
    }
}
