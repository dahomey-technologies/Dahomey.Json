using System;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class MaxDepthTests
    {
        public class Inner
        {
            public int Id { get; set; }
        }

        public class Outer
        {
            public Inner Inner { get; set; }
        }

        [Theory]
        [InlineData("[{}]", 0, null)]
        [InlineData("[{}]", 2, null)]
        [InlineData("[{}]", 1, typeof(JsonException))]
        public void ReadTest(string json, int maxDepth, Type expectedExceptionType)
        {
            JsonNode obj = JsonSerializer.Deserialize<JsonNode>(json, new JsonSerializerOptions().SetupExtensions());

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                MaxDepth = maxDepth
            }.SetupExtensions();

            Helper.TestRead(json, obj, options, expectedExceptionType);
        }

        [Theory]
        [InlineData("[{}]", 0, null)]
        [InlineData("[{}]", 2, null)]
        [InlineData("[{}]", 1, typeof(JsonException))]
        public void WriteTest(string json, int maxDepth, Type expectedExceptionType)
        {
            JsonNode obj = JsonSerializer.Deserialize<JsonNode>(json, new JsonSerializerOptions().SetupExtensions());

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                MaxDepth = maxDepth
            }.SetupExtensions();

            Helper.TestWrite(obj, json, options, expectedExceptionType);
        }
    }
}
