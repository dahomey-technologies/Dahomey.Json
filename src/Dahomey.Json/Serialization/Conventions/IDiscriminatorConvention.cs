using System;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Conventions
{
    public interface IDiscriminatorConvention
    {
        ReadOnlySpan<byte> MemberName { get; }
        bool TryRegisterType(Type type);
        Type ReadDiscriminator(ref Utf8JsonReader reader);
        void WriteDiscriminator(Utf8JsonWriter writer, Type actualType);
    }
}
