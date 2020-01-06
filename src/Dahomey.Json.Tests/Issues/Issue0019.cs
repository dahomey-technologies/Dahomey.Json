using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    /// <summary>
    /// https://github.com/dahomey-technologies/Dahomey.Json/issues/19
    /// </summary>
    public class Issue0019
    {
        public class Test
        {
            public object Property { get; set; } = "Test";
        }

        [Fact]
        public void Read()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""Property"":""Test""}";
            var test = JsonSerializer.Deserialize<Test>(json, options);
                
            Assert.NotNull(test);
            Assert.Equal("Test", test.Property);
        }

        [Fact]
        public void Write()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            const string json = @"{""Property"":""Test""}";
            Helper.TestWrite(new Test(), json, options);
        }
    }
}
