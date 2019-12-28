using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public abstract class AbstractCollectionConverter<TC, TI> : JsonConverter<TC>
        where TC : IEnumerable<TI>
    {
        private readonly JsonConverter<TI> _itemConverter;

        public AbstractCollectionConverter(JsonSerializerOptions options)
        {
            _itemConverter = options.GetConverter<TI>();
        }

        protected abstract ICollection<TI> InstantiateWorkingCollection();
        protected abstract TC InstantiateCollection(ICollection<TI> workingCollection);

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
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException();
                }

                ICollection<TI> workingCollection = InstantiateWorkingCollection();

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    TI item = _itemConverter.Read(ref reader, typeof(TI), options);
                    workingCollection.Add(item);
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
                writer.WriteStartArray();

                foreach (TI item in value)
                {
                    _itemConverter.Write(writer, item, options);
                }

                writer.WriteEndArray();
            }
        }
    }
}
