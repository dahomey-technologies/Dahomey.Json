using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0041
    {
        //[JsonConverter(typeof(ProblemDetailsJsonConverter))]
        public class ProblemDetails
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("status")]
            public int? Status { get; set; }

            [JsonPropertyName("detail")]
            public string Detail { get; set; }

            [JsonPropertyName("instance")]
            public string Instance { get; set; }

            [JsonExtensionData]
            public IDictionary<string, object> Extensions { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            object value = new ProblemDetails() { Status = 400 };
            const string json = @"{""type"":null,""title"":null,""status"":400,""detail"":null,""instance"":null}";

            string result = JsonSerializer.Serialize(value, value.GetType(), options);
            Assert.Equal(json, result);
        }

        [Fact]
        public void TestRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""type"":null,""title"":null,""status"":400,""detail"":null,""instance"":null}";

            ProblemDetails value = JsonSerializer.Deserialize<ProblemDetails>(json, options);
            Assert.NotNull(value);
            Assert.Equal(400, value.Status);
        }
    }
}
