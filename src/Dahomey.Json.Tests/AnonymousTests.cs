using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class AnonymousTests
    {
        [Fact]
        public void ReadAnonymous()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Id"":12,""Name"":""foo""}";
            var prototype = new { Id = default(int), Name = default(string) };

            var obj = JsonSerializerExtensions.DeserializeAnonymousType(json, prototype, options);

            Assert.Equal(12, obj.Id);
            Assert.Equal("foo", obj.Name);
        }

        [Fact]
        public void WriteAnonymous()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Id"":12,""Name"":""foo""}";
            var obj = new { Id = 12, Name = "foo" };

            Helper.TestWrite(obj, json, options);
        }
    }
}
