using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class BaseObjectTests
    {
        public class Object
        {
            public object Value { get; set; }
        }

        [Theory]
        [InlineData(@"{""Value"":12}", 12L)]
        [InlineData(@"{""Value"":18446744073709551615}", ulong.MaxValue)]
        [InlineData(@"{""Value"":12.25}", 12.25)]
        [InlineData(@"{""Value"":null}", null)]
        [InlineData(@"{""Value"":""foo""}", "foo")]
        [InlineData(@"{""Value"":true}", true)]
        [InlineData(@"{""Value"":false}", false)]
        public void Read(string json, object expectedValue)
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            var obj = JsonSerializer.Deserialize<Object>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(expectedValue, obj.Value);
        }

        [Fact]
        public void ReadObject()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""Value"":{""Id"":12}}";
            var obj = JsonSerializer.Deserialize<Object>(json, options);

            Assert.NotNull(obj);
            JsonObject jsonObject = Assert.IsType<JsonObject>(obj.Value);
            JsonNumber jsonNumber = Assert.IsType<JsonNumber>(jsonObject["Id"]);
            Assert.Equal(12, jsonNumber.GetInt32());
        }

        [Fact]
        public void ReadArray()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""Value"":[1,2,3]}";
            var obj = JsonSerializer.Deserialize<Object>(json, options);

            Assert.NotNull(obj);
            JsonArray jsonArray = Assert.IsType<JsonArray>(obj.Value);
            Assert.Equal(new JsonArray { 1, 2, 3 }, jsonArray);
        }

        [Theory]
        [InlineData(@"{""Value"":12}", 12L)]
        [InlineData(@"{""Value"":18446744073709551615}", ulong.MaxValue)]
        [InlineData(@"{""Value"":12.25}", 12.25)]
        [InlineData(@"{""Value"":null}", null)]
        [InlineData(@"{""Value"":""foo""}", "foo")]
        [InlineData(@"{""Value"":true}", true)]
        [InlineData(@"{""Value"":false}", false)]
        public void Write(string expectedJson, object value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            var obj = new Object
            {
                Value = value
            };

            Helper.TestWrite(obj, expectedJson, options);
        }

        [Fact]
        public void WriteObject()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""Value"":{""Id"":12}}";
            Object obj = new Object
            {
                Value = new JsonObject()
                {
                    ["Id"] = 12
                }
            };

            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void WriteArray()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""Value"":[1,2,3]}";
            Object obj = new Object
            {
                Value = new JsonArray { 1, 2, 3 }
            };

            Helper.TestWrite(obj, json, options);
        }
    }
}
