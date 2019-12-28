using Dahomey.Json.Util;
using System;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.DictionaryKeys
{
    public class DictionaryKeyConverterRegistry
    {
        private readonly JsonSerializerOptions _options;
        private readonly ConcurrentDictionary<Type, object> _dictionaryKeyConverters = new ConcurrentDictionary<Type, object>();

        public DictionaryKeyConverterRegistry(JsonSerializerOptions options)
        {
            _options = options;
        }

        public IDictionaryKeyConverter<T> GetDictionaryKeyConverter<T>()
        {
            return (IDictionaryKeyConverter<T>)_dictionaryKeyConverters
                .GetOrAdd(typeof(T), t => GenerateDictionaryKeyConverter<T>());
        }

        public void RegisterDictionaryKeyConverter<T>(IDictionaryKeyConverter<T> dictionaryKeyConverter)
        {
            _dictionaryKeyConverters.TryAdd(typeof(T), dictionaryKeyConverter);
        }

        private IDictionaryKeyConverter<T> GenerateDictionaryKeyConverter<T>()
        {
            Type type = typeof(T);

            if (type.IsEnum)
            {
                return new EnumDictionaryKeyConverter<T>();
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return new Utf8DictionaryKeyConverter<T>();

                case TypeCode.String:
                    return (IDictionaryKeyConverter<T>)new StringDictionaryKeyConverter();
            }

            throw new JsonException($"Cannot find a DictionaryKeyConverter for type {type}");
        }
    }
}
