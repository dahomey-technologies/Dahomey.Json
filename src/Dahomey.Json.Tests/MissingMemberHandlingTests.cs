using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class MissingMemberHandlingTests
    {
        public class Account
        {
            public string FullName { get; set; }
            public bool Deleted { get; set; }
        }

        [Fact]
        public void TestMissingMemberHandlingIgnore()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetMissingMemberHandling(MissingMemberHandling.Ignore);

            const string json = @"{""FullName"":""Dan Deleted"",""Deleted"":true,""DeletedDate"":""2013-01-20T00:00:00""}";
            var obj = JsonSerializer.Deserialize<Account>(json, options);
            Assert.NotNull(obj);
            Assert.Equal("Dan Deleted", obj.FullName);
            Assert.True(obj.Deleted);
        }

        [Fact]
        public void TestMissingMemberHandlingError()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetMissingMemberHandling(MissingMemberHandling.Error);

            const string json = @"{""FullName"":""Dan Deleted"",""Deleted"":true,""DeletedDate"":""2013-01-20T00:00:00""}";
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Account>(json, options));
        }
    }
}
