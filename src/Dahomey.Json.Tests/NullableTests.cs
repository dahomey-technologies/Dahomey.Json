using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class NullableTests
    {
        public class ObjectWithNullable
        {
            public int Id { get; set; }
            public int? Nullable1 { get; set; }
            public int? Nullable2 { get; set; }
        }

        [Fact]
        public void TestRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""Id"":12,""Nullable1"":13,""Nullable2"":null}";
            var obj = JsonSerializer.Deserialize<ObjectWithNullable>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(12, obj.Id);
            Assert.Equal(13, obj.Nullable1);
            Assert.Null(obj.Nullable2);
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""Id"":12,""Nullable1"":13,""Nullable2"":null}";
            var obj = new ObjectWithNullable
            {
                Id = 12,
                Nullable1 = 13
            };

            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void TestWriteIgnoreNullValues()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            }.SetupExtensions();

            const string json = @"{""Id"":12,""Nullable1"":13}";
            var obj = new ObjectWithNullable
            {
                Id = 12,
                Nullable1 = 13
            };

            Helper.TestWrite(obj, json, options);
        }
    }
}
