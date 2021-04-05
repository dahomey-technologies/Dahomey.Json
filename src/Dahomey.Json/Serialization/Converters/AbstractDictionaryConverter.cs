using Dahomey.Json.Serialization.Converters.DictionaryKeys;
using Dahomey.Json.Util;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public abstract class AbstractDictionaryConverter<TC, TK, TV> : AbstractJsonConverter<TC>
        where TK : notnull
        where TC : IEnumerable<KeyValuePair<TK, TV>>
    {
        private readonly IDictionaryKeyConverter<TK> _keyConverter;
        private readonly JsonConverter<TV> _valueConverter;
        private readonly ReferenceHandling _referenceHandling;

        protected abstract IDictionary<TK, TV> InstantiateWorkingCollection();
        protected abstract TC InstantiateCollection(IDictionary<TK, TV> workingCollection);

        public AbstractDictionaryConverter(JsonSerializerOptions options)
        {
            _keyConverter = options.GetDictionaryKeyConverterRegistry().GetDictionaryKeyConverter<TK>();
            _valueConverter = (JsonConverter<TV>)options.GetConverter(typeof(TV));
            _referenceHandling = typeof(TV).IsStruct() ? ReferenceHandling.Default : options.GetReferenceHandling();
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(TC);
        }

        public override TC Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default!;
            }

            TC collection = default!;

            Read(ref reader, ref collection, options);

            return collection;
        }

        public override void Read(ref Utf8JsonReader reader, ref TC obj, JsonSerializerOptions options)
        {
            using (new DepthHandler(options))
            {
                string? id = null;
                bool end = false;

                IDictionary<TK, TV> workingCollection = InstantiateWorkingCollection();

                if (_referenceHandling == ReferenceHandling.Preserve)
                {
                    reader.Read();

                    switch(reader.TokenType)
                    {
                        case JsonTokenType.EndObject:
                            end = true;
                            break;

                        case JsonTokenType.PropertyName:
                            {
                                ReadOnlySpan<byte> memberName = reader.GetRawString();

                                if (memberName.SequenceEqual(ReferenceHandler.ID_MEMBER_NAME))
                                {
                                    reader.Read();
                                    id = reader.GetString();
                                }
                                else if (memberName.SequenceEqual(ReferenceHandler.REF_MEMBER_NAME))
                                {
                                    reader.Read();
                                    string? @ref = reader.GetString();

                                    if (@ref == null)
                                    {
                                        throw new JsonException($"Cannot resolve null reference");
                                    }

                                    object? @object = SerializationContext.Current.ReferenceHandler.ResolveReference(@ref);

                                    if (@object == null)
                                    {
                                        throw new JsonException($"Cannot resolve reference {@ref}");
                                    }

                                    reader.Read();
                                    if (reader.TokenType != JsonTokenType.EndObject)
                                    {
                                        throw new JsonException("Expected end of object");
                                    }

                                    obj = InstantiateCollection((IDictionary<TK, TV>)@object);
                                    return;
                                }
                                else
                                {
                                    TK key = _keyConverter.Read(ref reader, options);

                                    reader.Read();
                                    TV value = _valueConverter.Read(ref reader, typeof(TV), options);

                                    workingCollection.Add(key, value!);
                                }
                            }
                            break;
                        default:
                            throw new JsonException();
                    }

                }
                else if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                if (!string.IsNullOrEmpty(id))
                {
                    SerializationContext.Current.ReferenceHandler.AddReference(workingCollection, id);
                }

                if (!end)
                {
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName)
                        {
                            throw new JsonException();
                        }

                        TK key = _keyConverter.Read(ref reader, options);

                        reader.Read();
                        TV value = _valueConverter.Read(ref reader, typeof(TV), options);

                        workingCollection.Add(key, value!);
                    }
                }

                if (obj == null || obj is IImmutableDictionary<TK, TV>)
                {
                    obj = InstantiateCollection(workingCollection);
                }
                else if (obj is IDictionary<TK, TV> collection)
                {
                    foreach (KeyValuePair<TK, TV> item in workingCollection)
                    {
                        collection.Add(item);
                    }
                }
                else
                {
                    throw new JsonException("Read only collection property not writable.");
                }
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

                if (_referenceHandling == ReferenceHandling.Ignore)
                {
                    ReferenceHandler referenceResolver = SerializationContext.Current.ReferenceHandler;

                    if (referenceResolver.IsReferenced(value))
                    {
                        writer.WriteNullValue();
                        return;
                    }
                    else
                    {
                        referenceResolver.AddReference(value);
                    }
                }
                else if (_referenceHandling == ReferenceHandling.Preserve)
                {
                    ReferenceHandler referenceResolver = SerializationContext.Current.ReferenceHandler;
                    string reference = referenceResolver.GetReference(value, out bool firstReference);

                    if (firstReference)
                    {
                        writer.WriteString(ReferenceHandler.ID_MEMBER_NAME, reference);
                    }
                    else
                    {
                        writer.WriteString(ReferenceHandler.REF_MEMBER_NAME, reference);
                        writer.WriteEndObject();
                        return;
                    }
                }

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
