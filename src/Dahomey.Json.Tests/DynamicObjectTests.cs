using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class DynamicObjectTests
    {
        [Fact]
        public void Read()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json =
                @"{""String"":""foo"",""Number"":12.12,""Bool"":true,""Null"":null,""Array"":[1,2],""Object"":{""Id"":1}}";

            dynamic actualObject = Helper.Read<JsonObject>(json, options);

            Assert.NotNull(actualObject);
            Assert.Equal("foo", (string)actualObject.String);
            Assert.Equal(12.12, (double)actualObject.Number, 3);
            Assert.True((bool)actualObject.Bool);
            Assert.Equal(JsonValueKind.Null, actualObject.Null.ValueKind);
            Assert.Equal(1, actualObject.Array[0].GetDouble());
            Assert.Equal(2, actualObject.Array[1].GetDouble());
            Assert.Equal(1, actualObject.Object.Id.GetInt32());
        }

        [Fact]
        public void Write()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json =
                @"{""String"":""foo"",""Number"":12.12,""Bool"":true,""Null"":null,""Array"":[1,2],""Object"":{""Id"":1}}";

            dynamic obj = new JsonObject();
            obj.String = "foo";
            obj.Number = 12.12;
            obj.Bool = true;
            obj.Null = null;
            obj.Array = new [] { 1, 2 };
            obj.Object = new JsonObject();
            obj.Object.Id = 1;

            Helper.TestWrite(obj, json, options);
        }
    }
}
