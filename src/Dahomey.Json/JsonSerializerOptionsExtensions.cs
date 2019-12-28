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
            options.Converters.Add(new JsonSerializerOptionsState(options));
            options.Converters.Add(new DictionaryConverterFactory());
            options.Converters.Add(new CollectionConverterFactory());
            options.Converters.Add(new JsonNodeConverterFactory());
            options.Converters.Add(new ObjectConverterFactory());

            return options;
        }

        public static ObjectMappingRegistry GetObjectMappingRegistry(this JsonSerializerOptions options)
        {
            return options.GetState().ObjectMappingRegistry;
        }

        public static ObjectMappingConventionRegistry GetObjectMappingConventionRegistry(this JsonSerializerOptions options)
        {
            return options.GetState().ObjectMappingConventionRegistry;
        }

        public static DiscriminatorConventionRegistry GetDiscriminatorConventionRegistry(this JsonSerializerOptions options)
        {
            return options.GetState().DiscriminatorConventionRegistry;
        }

        public static DictionaryKeyConverterRegistry GetDictionaryKeyConverterRegistry(this JsonSerializerOptions options)
        {
            return options.GetState().DictionaryKeyConverterRegistry;
        }

        private static JsonSerializerOptionsState GetState(this JsonSerializerOptions options)
        {
            return (JsonSerializerOptionsState)options.GetConverter<JsonSerializerOptionsState>();
        }
    }
}
