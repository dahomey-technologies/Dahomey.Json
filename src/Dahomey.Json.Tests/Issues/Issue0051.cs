using System;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0051
    {
        public class MyClass
        {
            public Uri Absolute { get; set; }
            public Uri Relative { get; set; }
        }

        [Fact]
        public void TestWrite()
        {
            MyClass myClass = new MyClass
            {
                Absolute = new Uri("http://acme.com"),
                Relative = new Uri("~/path", UriKind.Relative)
            };

            const string expectedJson = @"{""Absolute"":""http://acme.com"",""Relative"":""~/path""}";

            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            string actualJson = JsonSerializer.Serialize(myClass, options);
            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void TestRead()
        {
            const string actual = @"{""Absolute"":""http://acme.com"",""Relative"":""~/path""}";

            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            MyClass myClass = JsonSerializer.Deserialize<MyClass>(actual, options);
            Assert.NotNull(myClass);
            Assert.True(myClass.Absolute.IsAbsoluteUri);
            Assert.Equal("http://acme.com/", myClass.Absolute.AbsoluteUri);
            Assert.False(myClass.Relative.IsAbsoluteUri);
            Assert.Equal("~/path", myClass.Relative.OriginalString);
        }
    }
}
