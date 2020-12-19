using System;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class DefaultObjectMappingConventionProvider : IObjectMappingConventionProvider
    {
        private readonly DefaultObjectMappingConvention _defaultObjectMappingConvention = new DefaultObjectMappingConvention();

        public IObjectMappingConvention GetConvention(Type type)
        {
            return _defaultObjectMappingConvention;
        }
    }
}
