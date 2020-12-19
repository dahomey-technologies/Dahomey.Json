using Dahomey.Json.Util;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public abstract class AbstractCollectionConverter<TC, TI> : AbstractJsonConverter<TC>
        where TC : IEnumerable<TI>
    {
        private readonly JsonConverter<TI> _itemConverter;
        private readonly ReferenceHandling _referenceHandling;

        public AbstractCollectionConverter(JsonSerializerOptions options)
        {
            _itemConverter = options.GetConverter<TI>();
            _referenceHandling = typeof(TI).IsStruct() ? ReferenceHandling.Default : options.GetReferenceHandling();
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
                return default!;
            }

            TC collection = default!;

            Read(ref reader, ref collection, options);

            return collection;
        }

        public override void Read(ref Utf8JsonReader reader, ref TC obj, JsonSerializerOptions options)
        {
            using (ReferenceHandler referenceHandler = new ReferenceHandler(options))
            {
                string? id = null;

                if (_referenceHandling == ReferenceHandling.Preserve)
                {
                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException("Expected start of object");
                    }

                    reader.Read();
                    ReadOnlySpan<byte> memberName = reader.GetRawString();
                    JsonSerializerOptionsState state = options.GetState();

                    if (memberName.SequenceEqual(state.ID_MEMBER_NAME))
                    {
                        reader.Read();
                        id = reader.GetString();

                        reader.Read();
                        memberName = reader.GetRawString();
                        if (!memberName.SequenceEqual(state.VALUES_MEMBER_NAME))
                        {
                            throw new JsonException("Expected $values member name");
                        }
                        reader.Read();
                    }
                    else if (memberName.SequenceEqual(state.REF_MEMBER_NAME))
                    {
                        reader.Read();
                        string? @ref = reader.GetString();

                        if (@ref == null)
                        {
                            throw new JsonException($"Cannot resolve null reference");
                        }

                        object? @object = referenceHandler.ResolveReference(@ref);

                        if (@object == null)
                        {
                            throw new JsonException($"Cannot resolve reference {@ref}");
                        }

                        if (reader.TokenType != JsonTokenType.EndObject)
                        {
                            throw new JsonException("Expected end of object");
                        }

                        obj = InstantiateCollection((ICollection<TI>)@object);
                        return;
                    }
                    else
                    {
                        throw new JsonException($"Unexpected member name {Encoding.UTF8.GetString(memberName)}");
                    }
                }

                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException("Expected start of array");
                }

                ICollection<TI> workingCollection = InstantiateWorkingCollection();

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    TI item = _itemConverter.Read(ref reader, typeof(TI), options);
                    workingCollection.Add(item!);
                }

                if (_referenceHandling == ReferenceHandling.Preserve)
                {
                    reader.Read();
                    if (reader.TokenType != JsonTokenType.EndObject)
                    {
                        throw new JsonException("Expected end of object");
                    }
                }

                if (obj == null || obj is IImmutableList<TI> || obj is IImmutableSet<TI>)
                {
                    obj = InstantiateCollection(workingCollection);
                    if (!string.IsNullOrEmpty(id))
                    {
                        referenceHandler.AddReference(obj, id);
                    }
                }
                else if (obj is ICollection<TI> collection)
                {
                    foreach (TI item in workingCollection)
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

            using (ReferenceHandler referenceHandler = new ReferenceHandler(options))
            {
                if (_referenceHandling == ReferenceHandling.Preserve)
                {
                    writer.WriteStartObject();

                    string reference = referenceHandler.GetReference(value, out bool firstReference);
                    JsonSerializerOptionsState state = options.GetState();

                    if (firstReference)
                    {
                        writer.WriteString(state.ID_MEMBER_NAME, reference);
                        writer.WritePropertyName(state.VALUES_MEMBER_NAME);
                    }
                    else
                    {
                        writer.WriteString(state.REF_MEMBER_NAME, reference);
                        writer.WriteEndObject();
                        return;
                    }
                }

                writer.WriteStartArray();

                foreach (TI item in value)
                {
                    if (_referenceHandling == ReferenceHandling.Ignore)
                    {
                        if (referenceHandler!.IsReferenced(item!))
                        {
                            continue;
                        }
                        else
                        {
                            referenceHandler.AddReference(item!);
                        }
                    }

                    _itemConverter.Write(writer, item, options);
                }

                writer.WriteEndArray();

                if (_referenceHandling == ReferenceHandling.Preserve)
                {
                    writer.WriteEndObject();
                }
            }
        }
    }
}
