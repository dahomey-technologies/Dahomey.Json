using Dahomey.Json.Serialization.Converters.DictionaryKeys;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public abstract class AbstractDictionaryConverter<TC, TK, TV> : JsonConverter<TC>
        where TC : IDictionary<TK, TV>
    {
        private readonly IDictionaryKeyConverter<TK> _keyConverter;
        private readonly JsonConverter<TV> _valueConverter;

        protected abstract IDictionary<TK, TV> InstantiateWorkingCollection();
        protected abstract TC InstantiateCollection(IDictionary<TK, TV> workingCollection);

        public AbstractDictionaryConverter(JsonSerializerOptions options)
        {
            _keyConverter = options.GetDictionaryKeyConverterRegistry().GetDictionaryKeyConverter<TK>();
            _valueConverter = (JsonConverter<TV>)options.GetConverter(typeof(TV));
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(TC);
        }

        public override TC Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default(TC);
            }

            using (new DepthHandler(options))
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                IDictionary<TK, TV> workingCollection = InstantiateWorkingCollection();

                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    TK key = _keyConverter.Read(ref reader, options);

                    reader.Read();
                    TV value = _valueConverter.Read(ref reader, typeof(TV), options);

                    workingCollection.Add(key, value);
                }

                return InstantiateCollection(workingCollection);
            }
        }

        public override void Write(Utf8JsonWriter writer, TC value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            using (new DepthHandler(options))
            {
                writer.WriteStartObject();

                foreach (KeyValuePair<TK, TV> kvp in value)
                {
                    if (options.DictionaryKeyPolicy != null)
                    {
                        string key = options.DictionaryKeyPolicy.ConvertName(_keyConverter.ToString(kvp.Key));
                        writer.WritePropertyName(key);
                    }
                    else
                    {
                        _keyConverter.Write(writer, kvp.Key, options);
                    }

                    _valueConverter.Write(writer, kvp.Value, options);
                }

                writer.WriteEndObject();
            }
        }
    }
}
