using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0082
    {
        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string expected = @"""iVA=""";
            string actual = JsonSerializer.Serialize(new byte[] { 137, 80 }, options);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            byte[] expected = new byte[] { 137, 80 };
            byte[] actual = JsonSerializer.Deserialize<byte[]>(@"""iVA=""", options);
            Assert.Equal(expected, actual);
        }
    }
}
