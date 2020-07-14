using Dahomey.Json.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public class JsonNodeConverter : JsonConverter<JsonNode>
    {
        public override JsonNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    using (new DepthHandler(options))
                    {
                        return ReadObject(ref reader, options);
                    }

                case JsonTokenType.StartArray:
                    using (new DepthHandler(options))
                    {
                        return ReadArray(ref reader, options);
                    }

                case JsonTokenType.String:
                    return reader.GetString();

                case JsonTokenType.Number:
                    return new JsonNumber(Encoding.ASCII.GetString(reader.GetRawString()));

                case JsonTokenType.True:
                    return true;

                case JsonTokenType.False:
                    return false;

                case JsonTokenType.Null:
                    return JsonNull.Instance;

                default:
                    throw new JsonException();
            }
        }

        private JsonArray ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            JsonArray array = new JsonArray();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                JsonNode node = Read(ref reader, typeof(JsonNode), options);
                array.Add(node);
            }

            return array;
        }

        private JsonObject ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            JsonObject obj = new JsonObject();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                string? propertyName = reader.GetString();

                if (propertyName == null)
                {
                    throw new JsonException("Property name cannot be null");
                }

                reader.Read();
                JsonNode propertyValue = Read(ref reader, typeof(JsonNode), options);

                obj.Add(propertyName, propertyValue);
            }

            return obj;
        }

        public override void Write(Utf8JsonWriter writer, JsonNode value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    using (new DepthHandler(options))
                    {
                        WriteObject(writer, (JsonObject)value, options);
                    }
                    break;

                case JsonValueKind.Array:
                    using (new DepthHandler(options))
                    {
                        WriteArray(writer, (JsonArray)value, options);
                    }
                    break;

                case JsonValueKind.String:
                    writer.WriteStringValue(((JsonString)value).Value);
                    break;

                case JsonValueKind.Number:
                    writer.WriteNumberValue(Encoding.ASCII.GetBytes(value.ToString()));
                    break;

                case JsonValueKind.True:
                    writer.WriteBooleanValue(true);
                    break;

                case JsonValueKind.False:
                    writer.WriteBooleanValue(false);
                    break;

                case JsonValueKind.Null:
                    writer.WriteNullValue();
                    break;
            }
        }

        private void WriteObject(Utf8JsonWriter writer, JsonObject obj, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (KeyValuePair<string, JsonNode> kvp in obj)
            {
                writer.WritePropertyName(kvp.Key);
                Write(writer, kvp.Value, options);
            }

            writer.WriteEndObject();
        }

        private void WriteArray(Utf8JsonWriter writer, JsonArray array, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (JsonNode node in array)
            {
                Write(writer, node, options);
            }

            writer.WriteEndArray();
        }
    }
}
