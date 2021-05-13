using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0076
    {
        private class WrapperJsonConverter : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert)
            {
                if (!typeToConvert.IsGenericType)
                {
                    return false;
                }

                return typeToConvert.GetGenericTypeDefinition() == typeof(Wrapper<>);
            }

            public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
            {
                var innerType = type.GetGenericArguments()[0];
                var factoryType = typeof(WrapperJsonConverterInner<>).MakeGenericType(new Type[] {innerType});

                var converter = (JsonConverter)Activator.CreateInstance(
                    factoryType,
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    args: null,
                    culture: null);

                return converter;
            }

            private class WrapperJsonConverterInner<T> : JsonConverter<Wrapper<T>>
            {
                private const string WrapperPropertyName = "Wrapper";
                
                public override Wrapper<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                {
                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException();
                    }
                
                    reader.Read();
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        throw new JsonException();
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException();
                    }

                    var propertyName = reader.GetString();
                    if (propertyName != WrapperPropertyName)
                    {
                        throw new JsonException();
                    }
                
                    var value = JsonSerializer.Deserialize<T>(ref reader, options);

                    reader.Read();
                    if (reader.TokenType != JsonTokenType.EndObject)
                    {
                        throw new JsonException();
                    }

                    return new Wrapper<T> {Value = value};

                }

                public override void Write(Utf8JsonWriter writer, Wrapper<T> wrapper, JsonSerializerOptions options)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(WrapperPropertyName) ?? WrapperPropertyName);
                    JsonSerializer.Serialize(writer, wrapper.Value, options);
                    writer.WriteEndObject();
                }
            }
        }

        // Example of a class that needs custom converter since some field are not serializable.
        // Real life examples are various Maybe<T> implementations that serialize differently 
        // depending on their state.
        private sealed class Wrapper<T>
        {
            public bool PropertyThatThrows => throw new Exception("Do not access this property.");
            
            public T Value { set; get; }
        }

        private sealed class ClassWithWrapper
        {
            public Wrapper<string> WrappedValue { get; set; }
            
            public ClassWithWrapper()
            {
                WrappedValue = new Wrapper<string>();
            }
        }
        
        [Fact]
        public void TestSerializationWithFactoryConverter()
        {
            // Order of converters is important: custom one must be before Dahomey ones. 
            // Otherwise custom one will be not used and Dahomey's will fail with  
            // serialization due to PropertyThatThrows.
            var options = new JsonSerializerOptions();
            options.Converters.Add(new WrapperJsonConverter());
            options.SetupExtensions();

            const string expectedJson = @"{""WrappedValue"":{""Wrapper"":""test-value""}}";

            var obj = new ClassWithWrapper
            {
                WrappedValue = {Value = "test-value"}
            };
            var actualJson = JsonSerializer.Serialize(obj, options);

            Assert.Equal(expectedJson, actualJson);
        }
        
        [Fact]
        public void TestDeserializationWithFactoryConverter()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new WrapperJsonConverter());
            options.SetupExtensions();

            const string json = @"{""WrappedValue"":{""Wrapper"":""test-value""}}";
            var obj = JsonSerializer.Deserialize<ClassWithWrapper>(json, options);

            Assert.NotNull(obj);
            Assert.Equal("test-value", obj.WrappedValue.Value);
        }
    }
}