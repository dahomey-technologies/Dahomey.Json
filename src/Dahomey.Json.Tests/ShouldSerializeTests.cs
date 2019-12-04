using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class ShouldSerializeTests
    {
        private class ObjectWithShouldSerialize
        {
            public int Id { get; set; }

            public bool ShouldSerializeId()
            {
                return Id != 12;
            }
        }

        [Theory]
        [InlineData(12, "{}")] // {}
        [InlineData(13, @"{""Id"":13}")] // { "Id": 13}
        public void TestWrite(int id, string expected)
        {
            ObjectWithShouldSerialize obj = new ObjectWithShouldSerialize
            {
                Id = id
            };

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            string actual = JsonSerializer.Serialize(obj, options);

            Assert.Equal(expected, actual);
        }
    }
}
