#if NET5_0

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class DefaultIgnoreConditionTests
    {
        public class MyClass
        {
            public int Value { get; set; }

            public Version VersionValue { get; set; }
        }

        [Theory]
        [InlineData(JsonIgnoreCondition.Never, @"{""Value"":0,""VersionValue"":null}")]
        [InlineData(JsonIgnoreCondition.WhenWritingDefault, @"{}")]
        [InlineData(JsonIgnoreCondition.WhenWritingNull, @"{""Value"":0}")]
        public void DefaultIgnoreCondition(JsonIgnoreCondition jsonIgnoreCondition, string expectedJson)
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.DefaultIgnoreCondition = jsonIgnoreCondition;
            string actualJson = JsonSerializer.Serialize(new MyClass(), options);

            Assert.Equal(expectedJson, actualJson);
        }
    }
}

#endif