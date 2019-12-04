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
    }

    public class ExtensionDataMemberConverter<T, TValue> : IExtensionDataMemberConverter
    {
        private readonly Func<T, Dictionary<string, TValue>> _memberGetter;
        private readonly Action<T, Dictionary<string, TValue>> _memberSetter;
        private JsonConverter<JsonElement> _jsonElementConverter;


        public ExtensionDataMemberConverter(PropertyInfo propertyInfo, JsonSerializerOptions options)
        {
            Debug.Assert(propertyInfo.IsDefined(typeof(JsonExtensionDataAttribute)));
            _memberGetter = (Func<T, Dictionary<string, TValue>>)propertyInfo.GetMethod.CreateDelegate(typeof(Func<T, Dictionary<string, TValue>>));
            _memberSetter = (Action<T, Dictionary<string, TValue>>)propertyInfo.SetMethod.CreateDelegate(typeof(Action<T, Dictionary<string, TValue>>));
            _jsonElementConverter = options.GetConverter<JsonElement>();
        }

        public void Read(ref Utf8JsonReader reader, object obj, ReadOnlySpan<byte> memberName, JsonSerializerOptions options)
        {
            Dictionary<string, TValue> extensionData = _memberGetter((T)obj);
            if (extensionData == null)
            {
                extensionData = new Dictionary<string, TValue>();
                _memberSetter((T)obj, extensionData);
            }

            JsonElement jsonElement = _jsonElementConverter.Read(ref reader, typeof(JsonElement), options);
            extensionData[Encoding.UTF8.GetString(memberName)] = (TValue)(object)jsonElement;
        }
    }
}
