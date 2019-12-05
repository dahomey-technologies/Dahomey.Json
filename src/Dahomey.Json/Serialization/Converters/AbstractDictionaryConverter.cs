using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public abstract class AbstractDictionaryConverter<TC, TK, TV> : JsonConverter<TC>
        where TC : IDictionary<TK, TV>
    {
        private readonly JsonConverter<TV> _valueConverter;

        protected abstract IDictionary<TK, TV> InstantiateWorkingCollection();
        protected abstract TC InstantiateCollection(IDictionary<TK, TV> workingCollection);

        public AbstractDictionaryConverter(JsonSerializerOptions options)
        {
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

                string propertyName = reader.GetString();
                TK key = (TK)Convert.ChangeType(propertyName, typeof(TK));

                reader.Read();
                TV value = _valueConverter.Read(ref reader, typeof(TV), options);

                workingCollection.Add(key, value);
            }

            return InstantiateCollection(workingCollection);
        }

        public override void Write(Utf8JsonWriter writer, TC value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            foreach (KeyValuePair<TK, TV> kvp in value)
            {
                string key = kvp.Key.ToString();

                if (options.DictionaryKeyPolicy != null)
                {
                    key = options.DictionaryKeyPolicy.ConvertName(key);
                }

                writer.WritePropertyName(key);
                _valueConverter.Write(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}
