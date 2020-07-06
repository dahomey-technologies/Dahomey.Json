using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0042
    {
        public sealed class SerializableError : Dictionary<string, object>
        {
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            object value = new SerializableError { { "key1", 12 }, { "key2", "value" } };
            const string json = @"{""key1"":12,""key2"":""value""}";

            string result = JsonSerializer.Serialize(value, value.GetType(), options);
            Assert.Equal(json, result);
        }

        [Fact]
        public void TestRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""key1"":12,""key2"":""value""}";

            SerializableError value = JsonSerializer.Deserialize<SerializableError>(json, options);
            Assert.NotNull(value);
            Assert.Equal(2, value.Count);
            Assert.True(value.ContainsKey("key1"));
            Assert.True(value.ContainsKey("key2"));
            Assert.Equal(12L, value["key1"]);
            Assert.Equal("value", value["key2"]);
        }
    }
}
