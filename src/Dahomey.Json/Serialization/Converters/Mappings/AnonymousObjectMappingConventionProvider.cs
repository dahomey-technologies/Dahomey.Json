using Dahomey.Json.Util;
using System;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class AnonymousObjectMappingConventionProvider : IObjectMappingConventionProvider
    {
        private static readonly AnonymousObjectMappingConvention _anonymousObjectMappingConvention
            = new AnonymousObjectMappingConvention();

        public IObjectMappingConvention? GetConvention(Type type)
        {
            if (type.IsAnonymous())
            {
                return _anonymousObjectMappingConvention;
            }

            return null;
        }
    }
}
