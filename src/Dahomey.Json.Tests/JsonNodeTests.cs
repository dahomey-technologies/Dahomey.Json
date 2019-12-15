using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class JsonNodeTests
    {
        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public void ReadBoolean(string json, bool value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestRead(json, (JsonNode)value, options);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public void WriteBoolean(string json, bool value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestWrite((JsonNode)value, json, options);
        }

        [Theory]
        [InlineData("-9223372036854775808", long.MinValue)]
        [InlineData("-1000", -1000L)]
        [InlineData("0", 0L)]
        [InlineData("1000", 1000L)]
        [InlineData("9223372036854775807", long.MaxValue)]
        public void ReadInt64(string json, long expectedValue)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestRead(json, (JsonNode)expectedValue, options);
        }

        [Theory]
        [InlineData("-9223372036854775808", long.MinValue)]
        [InlineData("-1000", -1000L)]
        [InlineData("0", 0L)]
        [InlineData("1000", 1000L)]
        [InlineData("9223372036854775807", long.MaxValue)]
        public void WriteInt64(string json, long value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestWrite((JsonNode)value, json, options);
        }

        [Theory]
        [InlineData("0", 0ul)]
        [InlineData("1000", 1000ul)]
        [InlineData("18446744073709551615", ulong.MaxValue)]
        public void ReadUInt64(string json, ulong expectedValue)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestRead(json, (JsonNode)expectedValue, options);
        }

        [Theory]
        [InlineData("0", 0ul)]
        [InlineData("1000", 1000ul)]
        [InlineData("18446744073709551615", ulong.MaxValue)]
        public void WriteUInt64(string json, ulong value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestWrite((JsonNode)value, json, options);
        }

        [Theory]
        [InlineData("12.12", 12.12f)]
        //[InlineData(@"""NaN""", float.NaN)]
        //[InlineData(@"""∞""", float.PositiveInfinity)]
        //[InlineData(@"""-∞""", float.NegativeInfinity)]
        public void ReadSingle(string json, float expectedValue)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestRead(json, (JsonNode)expectedValue, options);
        }

        [Theory]
        [InlineData("12.12", 12.12f)]
        //[InlineData(@"""NaN""", float.NaN)]
        //[InlineData(@"""\u221E""", float.PositiveInfinity)]
        //[InlineData(@"""-\u221E""", float.NegativeInfinity)]
        public void WriteSingle(string json, float value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestWrite((JsonNode)value, json, options);
        }

        [Theory]
        [InlineData("12.12", 12.12)]
        //[InlineData(@"""NaN""", double.NaN)]
        //[InlineData(@"""∞""", double.PositiveInfinity)]
        //[InlineData(@"""-∞""", double.NegativeInfinity)]
        public void ReadDouble(string json, double expectedValue)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestRead(json, (JsonNode)expectedValue, options);
        }

        [Theory]
        [InlineData("12.12", 12.12)]
        //[InlineData(@"""NaN""", double.NaN)]
        //[InlineData(@"""\u221E""", double.PositiveInfinity)]
        //[InlineData(@"""-\u221E""", double.NegativeInfinity)]
        public void WriteDouble(string json, double value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestWrite((JsonNode)value, json, options);
        }

        [Theory]
        [InlineData(@"""foo""", "foo")]
        [InlineData(@"""""", "")]
        public void ReadString(string json, string expectedValue)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestRead(json, (JsonNode)expectedValue, options);
        }

        [Theory]
        [InlineData(@"""foo""", "foo")]
        [InlineData(@"""""", "")]
        public void WriteString(string json, string value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestWrite((JsonNode)value, json, options);
        }

        [Theory]
        [InlineData("[1,2,3,4]", "1,2,3,4")]
        public void ReadInt32List(string json, string expectedValue)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            JsonArray array = new JsonArray(expectedValue.Split(',').Select(s => (JsonNode)int.Parse(s)));
            Helper.TestRead(json, array, options);
        }

        [Theory]
        [InlineData("[1,2,3,4]", "1,2,3,4")]
        public void WriteInt32List(string json, string value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            JsonArray array = new JsonArray(value.Split(',').Select(s => (JsonNode)int.Parse(s)));
            Helper.TestWrite(array, json, options);
        }

        [Theory]
        [InlineData(@"[""aa"",""bb"",""cc"",""dd""]", "aa,bb,cc,dd")]
        public void ReadStringList(string json, string expectedValue)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            JsonArray array = new JsonArray(expectedValue.Split(',').Select(s => (JsonNode)s));
            Helper.TestRead(json, array, options);
        }

        [Theory]
        [InlineData(@"[""aa"",""bb"",""cc"",""dd""]", "aa,bb,cc,dd")]
        public void WriteStringList(string json, string value)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            JsonArray array = new JsonArray(value.Split(',').Select(s => (JsonNode)s));
            Helper.TestWrite(array, json, options);
        }

        [Fact]
        public void ReadObject()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json =
                @"{""string"":""foo"",""number"":12.12,""bool"":true,""null"":null,""array"":[1,2],""object"":{""id"":1}}";
            JsonObject actualObject = Helper.Read<JsonObject>(json, options);
            Assert.NotNull(actualObject);

            // pairs
            Assert.Equal(6, actualObject.GetPropertyNames().Count);

            // string
            JsonNode value = actualObject.GetPropertyValue("string");
            Assert.NotNull(value);
            Assert.Equal(JsonValueKind.String, value.ValueKind);
            Assert.IsType<JsonString>(value);
            Assert.Equal("foo", ((JsonString)value).Value);

            // number
            value = actualObject.GetPropertyValue("number");
            Assert.NotNull(value);
            Assert.Equal(JsonValueKind.Number, value.ValueKind);
            Assert.IsType<JsonNumber>(value);
            Assert.Equal(12.12, ((JsonNumber)value).GetDouble(), 3);

            // bool
            value = actualObject.GetPropertyValue("bool");
            Assert.NotNull(value);
            Assert.Equal(JsonValueKind.True, value.ValueKind);
            Assert.IsType<JsonBoolean>(value);
            Assert.True(((JsonBoolean)value).Value);

            // null
            value = actualObject.GetPropertyValue("null");
            Assert.NotNull(value);
            Assert.Equal(JsonValueKind.Null, value.ValueKind);
            Assert.IsType<JsonNull>(value);

            // array
            value = actualObject.GetPropertyValue("array");
            Assert.NotNull(value);
            Assert.Equal(JsonValueKind.Array, value.ValueKind);
            Assert.IsType<JsonArray>(value);
            JsonArray JsonArray = (JsonArray)value;
            Assert.Equal(2, JsonArray.Count);
            Assert.Equal(JsonValueKind.Number, JsonArray[0].ValueKind);
            Assert.Equal(1, ((JsonNumber)JsonArray[0]).GetDouble());
            Assert.Equal(JsonValueKind.Number, JsonArray[1].ValueKind);
            Assert.Equal(2, ((JsonNumber)JsonArray[1]).GetDouble());

            // object
            value = actualObject.GetPropertyValue("object");
            Assert.NotNull(value);
            Assert.Equal(JsonValueKind.Object, value.ValueKind);
            Assert.IsType<JsonObject>(value);
            JsonObject JsonObject = (JsonObject)value;
            value = JsonObject.GetPropertyValue("id");
            Assert.NotNull(value);
            Assert.Equal(JsonValueKind.Number, value.ValueKind);
            Assert.IsType<JsonNumber>(value);
            Assert.Equal(1, ((JsonNumber)value).GetInt32());
        }

        [Fact]
        public void WriteObject()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json =
                @"{""string"":""foo"",""number"":12.12,""bool"":true,""null"":null,""array"":[1,2],""object"":{""id"":1}}";

            JsonObject obj = new JsonObject
            {
                ["string"] = "foo",
                ["number"] = 12.12,
                ["bool"] = true,
                ["null"] = null,
                ["array"] = new JsonArray { 1, 2 },
                ["object"] = new JsonObject { ["id"] = 1 },
            };
            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void ReadArray()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"[""foo"",12.12,true,null,[1,2],{""id"":1}]";
            JsonArray actualArray = Helper.Read<JsonArray>(json, options);
            Assert.NotNull(actualArray);

            // values
            Assert.Equal(6, actualArray.Count);

            // string
            JsonNode actualString = actualArray[0];
            Assert.NotNull(actualString);
            Assert.Equal(JsonValueKind.String, actualString.ValueKind);
            Assert.IsType<JsonString>(actualString);
            Assert.Equal("foo", ((JsonString)actualString).Value);

            // number
            JsonNode actualNumber = actualArray[1];
            Assert.NotNull(actualNumber);
            Assert.Equal(JsonValueKind.Number, actualNumber.ValueKind);
            Assert.IsType<JsonNumber>(actualNumber);
            Assert.Equal(12.12, ((JsonNumber)actualNumber).GetDouble(), 3);

            // bool
            JsonNode actualBool = actualArray[2];
            Assert.NotNull(actualBool);
            Assert.Equal(JsonValueKind.True, actualBool.ValueKind);
            Assert.IsType<JsonBoolean>(actualBool);

            // null
            JsonNode actualNull = actualArray[3];
            Assert.NotNull(actualNull);
            Assert.Equal(JsonValueKind.Null, actualNull.ValueKind);

            // array
            JsonNode actualArrayValue = actualArray[4];
            Assert.NotNull(actualArrayValue);
            Assert.Equal(JsonValueKind.Array, actualArrayValue.ValueKind);
            Assert.IsType<JsonArray>(actualArrayValue);
            JsonArray JsonArray = (JsonArray)actualArrayValue;
            Assert.Equal(2, JsonArray.Count);
            Assert.Equal(JsonValueKind.Number, JsonArray[0].ValueKind);
            Assert.Equal(1, ((JsonNumber)JsonArray[0]).GetInt32());
            Assert.Equal(JsonValueKind.Number, JsonArray[1].ValueKind);
            Assert.Equal(2, ((JsonNumber)JsonArray[1]).GetInt32());

            // object
            JsonNode actualObject = actualArray[5];
            Assert.NotNull(actualObject);
            Assert.Equal(JsonValueKind.Object, actualObject.ValueKind);
            Assert.IsType<JsonObject>(actualObject);
            JsonObject JsonObject = (JsonObject)actualObject;
            JsonNode value = JsonObject.GetPropertyValue("id");
            Assert.NotNull(value);
            Assert.Equal(JsonValueKind.Number, value.ValueKind);
            Assert.IsType<JsonNumber>(value);
            Assert.Equal(1, ((JsonNumber)value).GetInt32());
        }

        [Fact]
        public void WriteArray()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"[""foo"",12.12,true,null,[1,2],{""id"":1}]";

            JsonArray array = new JsonArray
            {
                "foo",
                12.12,
                true,
                null,
                new JsonArray {1, 2},
                new JsonObject { { "id", 1 } }
            };
            Helper.TestWrite(array, json, options);
        }

        public class IdObject
        {
            public int Id { get; set; }
        }

        public class MyObject
        {
            public string String { get; set; }
            public double Number { get; set; }
            public bool Bool { get; set; }
            public object Null { get; set; }
            public List<int> Array { get; set; }
            public IdObject Object { get; set; }
        }

        [Fact]
        public void ToObjectTest()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            JsonObject obj = new JsonObject
            {
                ["String"] = "foo",
                ["Number"] = 12.12,
                ["Bool"] = true,
                ["Null"] = null,
                ["Array"] = new JsonArray { 1, 2 },
                ["Object"] = new JsonObject { ["Id"] = 1 },
            };

            MyObject myObject = obj.ToObject<MyObject>(options);

            Assert.NotNull(myObject);
            Assert.Equal("foo", myObject.String);
            Assert.Equal(12.12, myObject.Number, 3);
            Assert.True(myObject.Bool);
            Assert.Null(myObject.Null);
            Assert.Equal(new[] { 1, 2 }, myObject.Array);
            Assert.NotNull(myObject.Object);
            Assert.Equal(1, myObject.Object.Id);
        }
    }
}
