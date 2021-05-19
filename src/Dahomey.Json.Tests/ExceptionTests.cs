using System.Text.Json;
using System.Text.Json.Serialization;
using Dahomey.Json.Attributes;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class ExceptionTests
    {
        public class Inner {
            [JsonPropertyName("bar")]
            [JsonRequired]
            public long Bar { get; set; }
        }
        public class Outer
        {
            [JsonPropertyName("foo")]
            [JsonRequired]
            public Inner Foo { get; set; }
        }

        [Fact]
        public void TestException()
        {
            const string innerJson = @"{""bar"": ""a string instead of a number""}";
            string json = $"{{\"foo\": {innerJson}}}";

            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            JsonException exception = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Outer>(json, options));
            Assert.Equal("The JSON value could not be converted to System.Int64. Path: $.foo.bar", exception.Message);

            JsonException exception2 = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Inner>(innerJson, options));
            Assert.Equal("The JSON value could not be converted to System.Int64. Path: $.bar", exception2.Message);

            json = $"{{\"foo\": {{}}}}";
            JsonException exception3 = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Outer>(json, options));
            Assert.Equal("The JSON value could not be converted to Dahomey.Json.Tests.ExceptionTests+Inner due to: Required property 'bar' not found in JSON. Path: $.foo", exception3.Message);

            json = $"{{\"foo\": null}}";
            JsonException exception4 = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Outer>(json, options));
            Assert.Equal("Property 'foo' cannot be null.", exception4.Message);

            json = $"{{\"foo\": {{a}}}}";
            JsonException exception5 = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Outer>(json, options));
            Assert.StartsWith("The JSON value could not be converted to Dahomey.Json.Tests.ExceptionTests+Inner due to: 'a' is an invalid start of a property name.", exception5.Message);
        }

    }
}
