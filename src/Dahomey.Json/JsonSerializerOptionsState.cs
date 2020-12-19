using Dahomey.Json.Serialization;
using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Serialization.Converters.DictionaryKeys;
using Dahomey.Json.Serialization.Converters.Mappings;
using Dahomey.Json.Util;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Dahomey.Json
{
    internal class JsonSerializerOptionsState : JsonConverter<JsonSerializerOptionsState>
    {
        public ObjectMappingRegistry ObjectMappingRegistry { get; }
        public ObjectMappingConventionRegistry ObjectMappingConventionRegistry { get; }
        public DiscriminatorConventionRegistry DiscriminatorConventionRegistry { get; }
        public DictionaryKeyConverterRegistry DictionaryKeyConverterRegistry { get; }
        public ReferenceHandling ReferenceHandling { get; set; }
        public ReadOnlyPropertyHandling ReadOnlyPropertyHandling { get; set; }
        public MissingMemberHandling MissingMemberHandling { get; set; }
        public Utf8JsonWriterExtensions.WriteFormatedNumberValueFuncDelegate WriteFormatedNmberValueFunc { get; }
        public ConcurrentDictionary<Type, object?> DefaultValues { get; }
        public readonly byte[] ID_MEMBER_NAME = Encoding.ASCII.GetBytes("$id");
        public readonly byte[] REF_MEMBER_NAME = Encoding.ASCII.GetBytes("$ref");
        public readonly byte[] VALUES_MEMBER_NAME = Encoding.ASCII.GetBytes("$values");
        public AsyncLocal<ReferenceContext> CurrentReferenceContext { get; } = new AsyncLocal<ReferenceContext>();

        public JsonSerializerOptionsState(JsonSerializerOptions options)
        {
            ObjectMappingRegistry = new ObjectMappingRegistry(options);
            ObjectMappingConventionRegistry = new ObjectMappingConventionRegistry();
            DiscriminatorConventionRegistry = new DiscriminatorConventionRegistry(options);
            DictionaryKeyConverterRegistry = new DictionaryKeyConverterRegistry(options);

            MethodInfo? method = typeof(Utf8JsonWriter).GetMethod(
                nameof(Utf8JsonWriter.WriteNumberValue),
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(ReadOnlySpan<byte>) },
                null);

            if (method == null)
            {
                throw new JsonException("Unexpected");
            }

            WriteFormatedNmberValueFunc =
                (Utf8JsonWriterExtensions.WriteFormatedNumberValueFuncDelegate)method
                    .CreateDelegate(typeof(Utf8JsonWriterExtensions.WriteFormatedNumberValueFuncDelegate));

            DefaultValues = new ConcurrentDictionary<Type, object?>();
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
