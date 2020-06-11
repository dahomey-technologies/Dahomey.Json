using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0040
    {
        public sealed class JsonPayload
        {
            public string Application { get; set; }
            public IReadOnlyDictionary<string, string> Details { get; set; }
            public string Message { get; set; }
        }

        [Fact]
        public void TestReadIReadOnlyDictionary()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""Application"":""MyApp"",""Details"":{""Foo"":""foo"",""Bar"":""bar""},""Message"":""MyMessage""}";
            var obj = JsonSerializer.Deserialize<JsonPayload>(json, options);

            Assert.NotNull(obj);
            Assert.Equal("MyApp", obj.Application);
            Assert.Equal("MyMessage", obj.Message);
            Assert.NotNull(obj.Details);
            Assert.IsType<Dictionary<string, string>>(obj.Details);
            Assert.Equal(2, obj.Details.Count);
            Assert.Equal("foo", obj.Details["Foo"]);
            Assert.Equal("bar", obj.Details["Bar"]);
        }

        [Fact]
        public void TestWriteIReadOnlyDictionary()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            JsonPayload obj = new JsonPayload
            {
                Application = "MyApp",
                Details = new Dictionary<string, string>
                {
                    ["Foo"] = "foo",
                    ["Bar"] = "bar"
                },
                Message = "MyMessage"
            };

            const string expectedJson = @"{""Application"":""MyApp"",""Details"":{""Foo"":""foo"",""Bar"":""bar""},""Message"":""MyMessage""}";
            string actualJson = JsonSerializer.Serialize(obj, options);

            Assert.Equal(expectedJson, actualJson);
        }
    }
}
