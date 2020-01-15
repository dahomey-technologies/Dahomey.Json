using System;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public interface IObjectMappingConventionProvider
    {
        IObjectMappingConvention? GetConvention(Type type);
    }
}
