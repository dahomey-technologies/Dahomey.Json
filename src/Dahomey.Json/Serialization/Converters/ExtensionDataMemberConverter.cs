using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public interface IExtensionDataMemberConverter
    {
        void Read(ref Utf8JsonReader reader, object obj, ReadOnlySpan<byte> memberName, JsonSerializerOptions options);
        void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options);
    }

    public class ExtensionDataMemberConverter<T, TDict, TValue> : IExtensionDataMemberConverter
        where TDict : IDictionary<string, TValue>
    {
        private readonly Func<T, TDict> _memberGetter;
        private readonly Action<T, TDict>? _memberSetter;
        private JsonConverter<JsonElement> _jsonElementConverter;
        private JsonConverter<TValue> _jsonValueConverter;

        public ExtensionDataMemberConverter(PropertyInfo propertyInfo, JsonSerializerOptions options)
        {
            if (propertyInfo.GetMethod == null)
            {
                throw new JsonException("Invalid Serialization DataExtension Property");
            }

            Debug.Assert(propertyInfo.IsDefined(typeof(JsonExtensionDataAttribute)));
            _memberGetter = (Func<T, TDict>)propertyInfo.GetMethod.CreateDelegate(typeof(Func<T, TDict>));
            if (propertyInfo.SetMethod != null)
            {
                _memberSetter = (Action<T, TDict>)propertyInfo.SetMethod.CreateDelegate(typeof(Action<T, TDict>));
            }
            _jsonElementConverter = options.GetConverter<JsonElement>();
            _jsonValueConverter = options.GetConverter<TValue>();
        }

        public void Read(ref Utf8JsonReader reader, object obj, ReadOnlySpan<byte> memberName, JsonSerializerOptions options)
        {
            IDictionary<string, TValue> extensionData = _memberGetter((T)obj);

            if (extensionData == null)
            {
                extensionData = new Dictionary<string, TValue>();
                _memberSetter?.Invoke((T)obj, (TDict)extensionData);
            }

            JsonElement jsonElement = _jsonElementConverter.Read(ref reader, typeof(JsonElement), options);
            extensionData[Encoding.UTF8.GetString(memberName)] = (TValue)(object)jsonElement;
        }

        public void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options)
        {
            IDictionary<string, TValue> extensionData = _memberGetter((T)obj);
            if (extensionData == null)
            {
                return;
            }

            foreach (KeyValuePair<string, TValue> pair in extensionData)
            {
                string name = pair.Key;
                TValue value = pair.Value;

                writer.WritePropertyName(name);
                _jsonValueConverter.Write(writer, value, options);
            }
        }
    }
}
