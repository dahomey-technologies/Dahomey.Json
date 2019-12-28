using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Serialization.Converters.DictionaryKeys;
using Dahomey.Json.Serialization.Converters.Mappings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json
{
    public class JsonSerializerOptionsState : JsonConverter<JsonSerializerOptionsState>
    {
        public ObjectMappingRegistry ObjectMappingRegistry { get; }
        public ObjectMappingConventionRegistry ObjectMappingConventionRegistry { get; }
        public DiscriminatorConventionRegistry DiscriminatorConventionRegistry { get; }
        public DictionaryKeyConverterRegistry DictionaryKeyConverterRegistry { get; }

        public JsonSerializerOptionsState(JsonSerializerOptions options)
        {
            ObjectMappingRegistry = new ObjectMappingRegistry(options);
            ObjectMappingConventionRegistry = new ObjectMappingConventionRegistry();
            DiscriminatorConventionRegistry = new DiscriminatorConventionRegistry(options);
            DictionaryKeyConverterRegistry = new DictionaryKeyConverterRegistry(options);
        }

        public override JsonSerializerOptionsState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, JsonSerializerOptionsState value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
