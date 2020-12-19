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
        string? MemberName { get; set; }
        JsonConverter? Converter { get; set; }
        bool CanBeDeserialized { get; }
        bool CanBeSerialized { get; }
        object? DefaultValue { get; set; }
        bool IgnoreIfDefault { get; set; }
        Func<object, bool>? ShouldSerializeMethod { get; set; }
        RequirementPolicy RequirementPolicy { get; set; }
        IMemberConverter GenerateMemberConverter();
    }
}
