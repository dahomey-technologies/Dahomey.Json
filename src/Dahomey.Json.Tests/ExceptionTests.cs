using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class ExceptionTests
    {
        public class Inner {
            [JsonPropertyName("bar")]
            public long Bar { get; set; }
        }
        public class Outer
        {
            [JsonPropertyName("foo")]
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
        }
    }
}
