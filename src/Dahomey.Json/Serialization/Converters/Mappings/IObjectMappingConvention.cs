using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public interface IObjectMappingConvention
    {
        void Apply<T>(JsonSerializerOptions options, ObjectMapping<T> objectMapping) where T : class;
    }
}
