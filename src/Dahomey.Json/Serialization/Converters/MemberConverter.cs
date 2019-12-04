using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters
{
    public interface IMemberConverter
    {
        ReadOnlySpan<byte> MemberName { get; }
        string MemberNameAsString { get; }
        void Read(ref Utf8JsonReader reader, object obj, JsonSerializerOptions options);
        void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options);
        bool ShouldSerialize(object obj, JsonSerializerOptions options);
    }

    public class MemberConverter<T, TM> : IMemberConverter
        where T : class
    {
        Func<T, TM> _memberGetter;
        Action<T, TM> _memberSetter;
        JsonConverter<TM> _jsonConverter;
        ReadOnlyMemory<byte> _memberName;

        public ReadOnlySpan<byte> MemberName => _memberName.Span;
        public string MemberNameAsString { get; }

        public MemberConverter(PropertyInfo propertyInfo, JsonSerializerOptions options)
        {
            string name = GenerateMemberName(propertyInfo, options);
            MemberNameAsString = name;
            _memberName = Encoding.UTF8.GetBytes(name);

            _memberGetter = (Func<T, TM>)propertyInfo.GetMethod.CreateDelegate(typeof(Func<T, TM>));
            _memberSetter = (Action<T, TM>)propertyInfo.SetMethod.CreateDelegate(typeof(Action<T, TM>));
            _jsonConverter = (JsonConverter<TM>)options.GetConverter(typeof(TM));
        }

        public bool ShouldSerialize(object obj, JsonSerializerOptions options)
        {
            if (options.IgnoreNullValues && typeof(TM).IsClass && _memberGetter((T)obj) == null)
            {
                return false;
            }

            return true;
        }

        public void Read(ref Utf8JsonReader reader, object obj, JsonSerializerOptions options)
        {
            _memberSetter((T)obj, _jsonConverter.Read(ref reader, typeof(TM), options));
        }

        public void Write(Utf8JsonWriter writer, object obj, JsonSerializerOptions options)
        {
            _jsonConverter.Write(writer, _memberGetter((T)obj), options);
        }

        private string GenerateMemberName(PropertyInfo propertyInfo, JsonSerializerOptions options)
        {
            if (propertyInfo == null)
            {
                return null;
            }

            JsonPropertyNameAttribute nameAttribute = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>(inherit: false);
            if (nameAttribute != null)
            {
                return nameAttribute.Name ?? throw new JsonException();
            }
            else if (options.PropertyNamingPolicy != null)
            {
                return options.PropertyNamingPolicy.ConvertName(propertyInfo.Name) ?? throw new JsonException();
            }
            else
            {
                return propertyInfo.Name;
            }
        }
    }
}
