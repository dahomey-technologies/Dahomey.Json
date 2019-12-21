using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Serialization.Converters.DictionaryKeys;
using Dahomey.Json.Serialization.Converters.Factories;
using Dahomey.Json.Serialization.Converters.Mappings;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonConverter<T> GetConverter<T>(this JsonSerializerOptions options)
        {
            return (JsonConverter<T>)options.GetConverter(typeof(T));
        }

        public static JsonSerializerOptions SetupExtensions(this JsonSerializerOptions options)
        {
            options.Converters.Add(new ObjectMappingRegistry(options));
            options.Converters.Add(new ObjectMappingConventionRegistry());
            options.Converters.Add(new DiscriminatorConventionRegistry(options));
            options.Converters.Add(new DictionaryKeyConverterRegistry(options));
            options.Converters.Add(new DictionaryConverterFactory());
            options.Converters.Add(new CollectionConverterFactory());
            options.Converters.Add(new JsonNodeConverterFactory());
            options.Converters.Add(new ObjectConverterFactory());

            return options;
        }

        public static ObjectMappingRegistry GetObjectMappingRegistry(this JsonSerializerOptions options)
        {
            return (ObjectMappingRegistry)options.GetConverter<ObjectMappingRegistry>();
        }

        public static ObjectMappingConventionRegistry GetObjectMappingConventionRegistry(this JsonSerializerOptions options)
        {
            return (ObjectMappingConventionRegistry)options.GetConverter<ObjectMappingConventionRegistry>();
        }

        public static DiscriminatorConventionRegistry GetDiscriminatorConventionRegistry(this JsonSerializerOptions options)
        {
            return (DiscriminatorConventionRegistry)options.GetConverter<DiscriminatorConventionRegistry>();
        }

        public static DictionaryKeyConverterRegistry GetDictionaryKeyConverterRegistry(this JsonSerializerOptions options)
        {
            return (DictionaryKeyConverterRegistry)options.GetConverter<DictionaryKeyConverterRegistry>();
        }
    }
}
