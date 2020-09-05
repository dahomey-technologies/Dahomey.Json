using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class ExceptionTests
    {
        public class Holder
        {
            [JsonPropertyName("foo")]
            public long Foo { get; set; }
        }

        [Fact]
        public void TestException()
        {
            const string json = @"{""foo"": ""a string instead of a number""}";

            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            JsonException exception = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Holder>(json, options));
            Assert.Equal("The JSON value could not be converted to System.Int64. Path: $.foo", exception.Message);
        }
    }
}
