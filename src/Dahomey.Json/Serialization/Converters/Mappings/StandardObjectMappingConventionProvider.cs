using System;
using System.Runtime.Serialization;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class StandardObjectMappingConventionProvider : IObjectMappingConventionProvider
    {
        private static readonly StandardObjectMappingConvention _standardObjectMappingConvention = new StandardObjectMappingConvention();

        public IObjectMappingConvention GetConvention(Type type)
        {
            if (type.IsDefined(typeof(DataContractAttribute), inherit: false))
            {
                return _standardObjectMappingConvention;
            }

            return null;
        }
    }
}
