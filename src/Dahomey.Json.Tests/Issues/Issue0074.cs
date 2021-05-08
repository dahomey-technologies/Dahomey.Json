using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0074
    {
        public class ClassWithUri
        {
            public Uri UrisAreNullable { get; set; }
        }

        [Fact]
        public void TestWriteDahomey()
        {
            ClassWithUri objectWithUri = new ClassWithUri
            {
                UrisAreNullable = null,
            };

            const string expectedJson = @"{""UrisAreNullable"":null}";

            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            string actualJson = JsonSerializer.Serialize(objectWithUri, options);
            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void TestReadDahomey()
        {
            const string actual = @"{""UrisAreNullable"":null}";

            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            ClassWithUri objectWithUri = JsonSerializer.Deserialize<ClassWithUri>(actual, options);
            Assert.NotNull(objectWithUri);
            Assert.Null(objectWithUri.UrisAreNullable);
        }

        [Fact]
        public void TestWriteDefault()
        {
            ClassWithUri objectWithUri = new ClassWithUri
            {
                UrisAreNullable = null,
            };

            const string expectedJson = @"{""UrisAreNullable"":null}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            string actualJson = JsonSerializer.Serialize(objectWithUri, options);
            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void TestReadDefault()
        {
            const string actual = @"{""UrisAreNullable"":null}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            ClassWithUri objectWithUri = JsonSerializer.Deserialize<ClassWithUri>(actual, options);
            Assert.NotNull(objectWithUri);
            Assert.Null(objectWithUri.UrisAreNullable);
        }
    }
}
