using Dahomey.Json.Util;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class AnonymousObjectMappingConvention : IObjectMappingConvention
    {
        private static DefaultObjectMappingConvention _defaultObjectMappingConvention = new DefaultObjectMappingConvention();

        public void Apply<T>(JsonSerializerOptions options, ObjectMapping<T> objectMapping)
        {
            Debug.Assert(typeof(T).IsAnonymous());

            // anonymous types have a single non default constructor
            ConstructorInfo constructorInfo = typeof(T).GetConstructors()[0];

            _defaultObjectMappingConvention.Apply(options, objectMapping);
            objectMapping.MapCreator(constructorInfo);
        }
    }
}
