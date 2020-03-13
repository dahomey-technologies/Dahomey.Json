using Dahomey.Json.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class ObjectTests
    {
        public enum EnumTest
        {
            None = 0,
            Value1 = 1,
            Value2 = 2,
        }

        public class SimpleObject
        {
            public bool Boolean { get; set; }
            public sbyte SByte { get; set; }
            public byte Byte { get; set; }
            public ushort Int16 { get; set; }
            public short UInt16 { get; set; }
            public int Int32 { get; set; }
            public uint UInt32 { get; set; }
            public long Int64 { get; set; }
            public ulong UInt64 { get; set; }
            public string String { get; set; }
            public float Single { get; set; }
            public double Double { get; set; }
            public decimal Decimal { get; set; }
            public DateTime DateTime { get; set; }
            public EnumTest Enum { get; set; }
        }

        [Fact]
        public void ReadSimpleObject()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.Converters.Add(new JsonStringEnumConverter());

            const string json = @"{""Boolean"":true,""SByte"":13,""Byte"":12,""Int16"":14,""UInt16"":15,""Int32"":16,""UInt32"":17,""Int64"":18,""UInt64"":19,""String"":""string"",""Single"":20.209999084472656,""Double"":22.23,""Decimal"":12.12,""DateTime"":""2014-02-21T19:00:00Z"",""Enum"":""Value1""}";
            SimpleObject obj = JsonSerializer.Deserialize<SimpleObject>(json, options);

            Assert.NotNull(obj);
            Assert.True(obj.Boolean);
            Assert.Equal(12, obj.Byte);
            Assert.Equal(13, obj.SByte);
            Assert.Equal(14, obj.Int16);
            Assert.Equal(15, obj.UInt16);
            Assert.Equal(16, obj.Int32);
            Assert.Equal(17u, obj.UInt32);
            Assert.Equal(18, obj.Int64);
            Assert.Equal(19ul, obj.UInt64);
            Assert.Equal(20.21f, obj.Single);
            Assert.Equal(22.23, obj.Double);
            Assert.Equal(12.12m, obj.Decimal);
            Assert.Equal("string", obj.String);
            Assert.Equal(new DateTime(2014, 02, 21, 19, 0, 0, DateTimeKind.Utc), obj.DateTime);
            Assert.Equal(EnumTest.Value1, obj.Enum);
        }

        [Fact]
        public void WriteSimpleObject()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.Converters.Add(new JsonStringEnumConverter());

            SimpleObject obj = new SimpleObject
            {
                Boolean = true,
                Byte = 12,
                SByte = 13,
                Int16 = 14,
                UInt16 = 15,
                Int32 = 16,
                UInt32 = 17u,
                Int64 = 18,
                UInt64 = 19ul,
                Single = 20.25f,
                Double = 22.23,
                Decimal = 12.12m,
                String = "string",
                DateTime = new DateTime(2014, 02, 21, 19, 0, 0, DateTimeKind.Utc),
                Enum = EnumTest.Value1
            };

            const string expected = @"{""Boolean"":true,""SByte"":13,""Byte"":12,""Int16"":14,""UInt16"":15,""Int32"":16,""UInt32"":17,""Int64"":18,""UInt64"":19,""String"":""string"",""Single"":20.25,""Double"":22.23,""Decimal"":12.12,""DateTime"":""2014-02-21T19:00:00Z"",""Enum"":""Value1""}";
            string actual = JsonSerializer.Serialize(obj, options);

            Assert.Equal(expected, actual);
        }

        public class SimpleObjectWithFields
        {
            public bool Boolean;
            public sbyte SByte;
            public byte Byte;
            public ushort Int16;
            public short UInt16;
            public int Int32;
            public uint UInt32;
            public long Int64;
            public ulong UInt64;
            public string String;
            public float Single;
            public double Double;
            public DateTime DateTime;
            public EnumTest Enum;
        }

        [Fact]
        public void ReadSimpleObjectWithFields()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.Converters.Add(new JsonStringEnumConverter());

            const string json = @"{""Boolean"":true,""SByte"":13,""Byte"":12,""Int16"":14,""UInt16"":15,""Int32"":16,""UInt32"":17,""Int64"":18,""UInt64"":19,""String"":""string"",""Single"":20.209999084472656,""Double"":22.23,""DateTime"":""2014-02-21T19:00:00Z"",""Enum"":""Value1""}";
            SimpleObjectWithFields obj = JsonSerializer.Deserialize<SimpleObjectWithFields>(json, options);

            Assert.NotNull(obj);
            Assert.True(obj.Boolean);
            Assert.Equal(12, obj.Byte);
            Assert.Equal(13, obj.SByte);
            Assert.Equal(14, obj.Int16);
            Assert.Equal(15, obj.UInt16);
            Assert.Equal(16, obj.Int32);
            Assert.Equal(17u, obj.UInt32);
            Assert.Equal(18, obj.Int64);
            Assert.Equal(19ul, obj.UInt64);
            Assert.Equal(20.21f, obj.Single);
            Assert.Equal(22.23, obj.Double);
            Assert.Equal("string", obj.String);
            Assert.Equal(new DateTime(2014, 02, 21, 19, 0, 0, DateTimeKind.Utc), obj.DateTime);
            Assert.Equal(EnumTest.Value1, obj.Enum);
        }

        [Fact]
        public void WriteSimpleObjectWithFields()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.Converters.Add(new JsonStringEnumConverter());

            SimpleObjectWithFields obj = new SimpleObjectWithFields
            {
                Boolean = true,
                Byte = 12,
                SByte = 13,
                Int16 = 14,
                UInt16 = 15,
                Int32 = 16,
                UInt32 = 17u,
                Int64 = 18,
                UInt64 = 19ul,
                Single = 20.25f,
                Double = 22.23,
                String = "string",
                DateTime = new DateTime(2014, 02, 21, 19, 0, 0, DateTimeKind.Utc),
                Enum = EnumTest.Value1
            };

            const string expected = @"{""Boolean"":true,""SByte"":13,""Byte"":12,""Int16"":14,""UInt16"":15,""Int32"":16,""UInt32"":17,""Int64"":18,""UInt64"":19,""String"":""string"",""Single"":20.25,""Double"":22.23,""DateTime"":""2014-02-21T19:00:00Z"",""Enum"":""Value1""}";
            string actual = JsonSerializer.Serialize(obj, options);

            Assert.Equal(expected, actual);
        }

        public class ListObject
        {
            public List<int> IntList { get; set; }
            public List<IntObject> ObjectList { get; set; }
            public List<string> StringList { get; set; }
        }

        public class IntObject
        {
            public int IntValue { get; set; }

            protected bool Equals(IntObject other)
            {
                return IntValue == other.IntValue;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((IntObject)obj);
            }

            public override int GetHashCode()
            {
                return IntValue;
            }
        }

        [Fact]
        public void ReadObjectWithList()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""IntList"":[1,2],""ObjectList"":[{""IntValue"":1},{""IntValue"":2}],""StringList"":[""a"",""b""]}";
            ListObject obj = Helper.Read<ListObject>(json, options);

            Assert.NotNull(obj);

            Assert.NotNull(obj.IntList);
            Assert.Equal(2, obj.IntList.Count);
            Assert.Equal(1, obj.IntList[0]);
            Assert.Equal(2, obj.IntList[1]);

            Assert.NotNull(obj.ObjectList);
            Assert.Equal(2, obj.ObjectList.Count);
            Assert.NotNull(obj.ObjectList[0]);
            Assert.Equal(1, obj.ObjectList[0].IntValue);
            Assert.NotNull(obj.ObjectList[1]);
            Assert.Equal(2, obj.ObjectList[1].IntValue);

            Assert.NotNull(obj.StringList);
            Assert.Equal(2, obj.StringList.Count);
            Assert.Equal("a", obj.StringList[0]);
            Assert.Equal("b", obj.StringList[1]);
        }

        [Fact]
        public void WriteWithList()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""IntList"":[1,2],""ObjectList"":[{""IntValue"":1},{""IntValue"":2}],""StringList"":[""a"",""b""]}";

            ListObject obj = new ListObject
            {
                IntList = new List<int> { 1, 2 },
                ObjectList = new List<IntObject>
                {
                    new IntObject { IntValue = 1 },
                    new IntObject { IntValue = 2 }
                },
                StringList = new List<string> { "a", "b" }
            };

            Helper.TestWrite(obj, json, options);
        }

        public class ArrayObject
        {
            public int[] IntArray { get; set; }
            public IntObject[] ObjectArray { get; set; }
            public string[] StringArray { get; set; }
        }

        [Fact]
        public void ReadWithArray()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""IntArray"":[1,2],""ObjectArray"":[{""IntValue"":1},{""IntValue"":2}],""StringArray"":[""a"",""b""]}";
            ArrayObject obj = Helper.Read<ArrayObject>(json, options);

            Assert.NotNull(obj);

            Assert.NotNull(obj.IntArray);
            Assert.Equal(2, obj.IntArray.Length);
            Assert.Equal(1, obj.IntArray[0]);
            Assert.Equal(2, obj.IntArray[1]);

            Assert.NotNull(obj.ObjectArray);
            Assert.Equal(2, obj.ObjectArray.Length);
            Assert.NotNull(obj.ObjectArray[0]);
            Assert.Equal(1, obj.ObjectArray[0].IntValue);
            Assert.NotNull(obj.ObjectArray[1]);
            Assert.Equal(2, obj.ObjectArray[1].IntValue);

            Assert.NotNull(obj.StringArray);
            Assert.Equal(2, obj.StringArray.Length);
            Assert.Equal("a", obj.StringArray[0]);
            Assert.Equal("b", obj.StringArray[1]);
        }

        [Fact]
        public void WriteWithArray()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""IntArray"":[1,2],""ObjectArray"":[{""IntValue"":1},{""IntValue"":2}],""StringArray"":[""a"",""b""]}";

            ArrayObject obj = new ArrayObject
            {
                IntArray = new[] { 1, 2 },
                ObjectArray = new[]
                {
                    new IntObject { IntValue = 1 },
                    new IntObject { IntValue = 2 }
                },
                StringArray = new[] { "a", "b" }
            };

            Helper.TestWrite(obj, json, options);
        }

        public class DictionaryObject
        {
            public Dictionary<int, int> IntDictionary { get; set; }
            public Dictionary<uint, IntObject> UIntDictionary { get; set; }
            public Dictionary<string, List<IntObject>> StringDictionary { get; set; }
            public Dictionary<EnumTest, Dictionary<int, IntObject>> EnumDictionary { get; set; }
        }

        [Fact]
        public void ReadWithDictionary()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.Converters.Add(new JsonStringEnumConverter());

            const string json = @"{""IntDictionary"":{""1"":1,""2"":2},""UIntDictionary"":{""1"":{""IntValue"":1},""2"":{""IntValue"":2}},""StringDictionary"":{""1"":[{""IntValue"":11},{""IntValue"":12}],""2"":[{""IntValue"":21},{""IntValue"":22}]},""EnumDictionary"":{""Value1"":{""1"":{""IntValue"":11},""2"":{""IntValue"":12}},""Value2"":{""1"":{""IntValue"":21},""2"":{""IntValue"":22}}}}";
            DictionaryObject obj = Helper.Read<DictionaryObject>(json, options);

            Assert.NotNull(obj);

            Assert.NotNull(obj.IntDictionary);
            Assert.Equal(2, obj.IntDictionary.Count);
            Assert.True(obj.IntDictionary.ContainsKey(1));
            Assert.Equal(1, obj.IntDictionary[1]);
            Assert.True(obj.IntDictionary.ContainsKey(2));
            Assert.Equal(2, obj.IntDictionary[2]);

            Assert.NotNull(obj.UIntDictionary);
            Assert.Equal(2, obj.UIntDictionary.Count);
            Assert.True(obj.UIntDictionary.ContainsKey(1));
            Assert.NotNull(obj.UIntDictionary[1]);
            Assert.Equal(1, obj.UIntDictionary[1].IntValue);
            Assert.True(obj.UIntDictionary.ContainsKey(2));
            Assert.NotNull(obj.UIntDictionary[2]);
            Assert.Equal(2, obj.UIntDictionary[2].IntValue);

            Assert.NotNull(obj.StringDictionary);
            Assert.Equal(2, obj.StringDictionary.Count);
            Assert.True(obj.StringDictionary.ContainsKey("1"));
            Assert.NotNull(obj.StringDictionary["1"]);
            Assert.Equal(2, obj.StringDictionary["1"].Count);
            Assert.Equal(11, obj.StringDictionary["1"][0].IntValue);
            Assert.Equal(12, obj.StringDictionary["1"][1].IntValue);
            Assert.True(obj.StringDictionary.ContainsKey("2"));
            Assert.NotNull(obj.StringDictionary["2"]);
            Assert.Equal(2, obj.StringDictionary["2"].Count);
            Assert.Equal(21, obj.StringDictionary["2"][0].IntValue);
            Assert.Equal(22, obj.StringDictionary["2"][1].IntValue);

            Assert.NotNull(obj.EnumDictionary);
            Assert.Equal(2, obj.EnumDictionary.Count);
            Assert.True(obj.EnumDictionary.ContainsKey(EnumTest.Value1));
            Assert.NotNull(obj.EnumDictionary[EnumTest.Value1]);
            Assert.Equal(2, obj.EnumDictionary[EnumTest.Value1].Count);
            Assert.Equal(11, obj.EnumDictionary[EnumTest.Value1][1].IntValue);
            Assert.Equal(12, obj.EnumDictionary[EnumTest.Value1][2].IntValue);
            Assert.True(obj.EnumDictionary.ContainsKey(EnumTest.Value2));
            Assert.NotNull(obj.EnumDictionary[EnumTest.Value2]);
            Assert.Equal(2, obj.EnumDictionary[EnumTest.Value2].Count);
            Assert.Equal(21, obj.EnumDictionary[EnumTest.Value2][1].IntValue);
            Assert.Equal(22, obj.EnumDictionary[EnumTest.Value2][2].IntValue);
        }

        [Fact]
        public void WriteWithDictionary()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.Converters.Add(new JsonStringEnumConverter());

            const string json = @"{""IntDictionary"":{""1"":1,""2"":2},""UIntDictionary"":{""1"":{""IntValue"":1},""2"":{""IntValue"":2}},""StringDictionary"":{""1"":[{""IntValue"":11},{""IntValue"":12}],""2"":[{""IntValue"":21},{""IntValue"":22}]},""EnumDictionary"":{""Value1"":{""1"":{""IntValue"":11},""2"":{""IntValue"":12}},""Value2"":{""1"":{""IntValue"":21},""2"":{""IntValue"":22}}}}";

            DictionaryObject obj = new DictionaryObject
            {
                IntDictionary = new Dictionary<int, int>
                {
                    { 1, 1 },
                    { 2, 2 }
                },
                UIntDictionary = new Dictionary<uint, IntObject>
                {
                    { 1, new IntObject { IntValue = 1 } },
                    { 2, new IntObject { IntValue = 2 } }
                },
                StringDictionary = new Dictionary<string, List<IntObject>>
                {
                    { "1", new List<IntObject> { new IntObject { IntValue = 11 }, new IntObject { IntValue = 12 } } },
                    { "2", new List<IntObject> { new IntObject { IntValue = 21 }, new IntObject { IntValue = 22 } } }
                },
                EnumDictionary = new Dictionary<EnumTest, Dictionary<int, IntObject>>
                {
                    {
                        EnumTest.Value1, new Dictionary<int, IntObject>
                        {
                            { 1, new IntObject { IntValue = 11 } },
                            { 2, new IntObject { IntValue = 12 } }
                        }
                    },
                    {
                        EnumTest.Value2, new Dictionary<int, IntObject>
                        {
                            { 1, new IntObject { IntValue = 21 } },
                            { 2, new IntObject { IntValue = 22 } }
                        }
                    }
                }
            };

            Helper.TestWrite(obj, json, options);
        }

        public class HashSetObject
        {
            public HashSet<int> IntHashSet { get; set; }
            public HashSet<IntObject> ObjectHashSet { get; set; }
            public HashSet<string> StringHashSet { get; set; }
        }

        [Fact]
        public void ReadWithHashSet()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""IntHashSet"":[1,2],""ObjectHashSet"":[{""IntValue"":1},{""IntValue"":2}],""StringHashSet"":[""a"",""b""]}";
            HashSetObject obj = Helper.Read<HashSetObject>(json, options);

            Assert.NotNull(obj);

            Assert.NotNull(obj.IntHashSet);
            Assert.Equal(2, obj.IntHashSet.Count);
            Assert.Contains(1, obj.IntHashSet);
            Assert.Contains(2, obj.IntHashSet);

            Assert.NotNull(obj.ObjectHashSet);
            Assert.Equal(2, obj.ObjectHashSet.Count);
            Assert.Contains(new IntObject { IntValue = 1 }, obj.ObjectHashSet);
            Assert.Contains(new IntObject { IntValue = 2 }, obj.ObjectHashSet);

            Assert.NotNull(obj.StringHashSet);
            Assert.Equal(2, obj.StringHashSet.Count);
            Assert.Contains("a", obj.StringHashSet);
            Assert.Contains("b", obj.StringHashSet);
        }

        [Fact]
        public void WriteWithHashSet()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""IntHashSet"":[1,2],""ObjectHashSet"":[{""IntValue"":1},{""IntValue"":2}],""StringHashSet"":[""a"",""b""]}";

            HashSetObject obj = new HashSetObject
            {
                IntHashSet = new HashSet<int> { 1, 2 },
                ObjectHashSet = new HashSet<IntObject>
                {
                    new IntObject { IntValue = 1 },
                    new IntObject { IntValue = 2 }
                },
                StringHashSet = new HashSet<string> { "a", "b" }
            };

            Helper.TestWrite(obj, json, options);
        }

        public class ObjectWithObject
        {
            public IntObject Object { get; set; }
        }

        [Fact]
        public void ReadWithObject()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Object"":{""IntValue"":12}}";
            ObjectWithObject obj = Helper.Read<ObjectWithObject>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Object);
            Assert.Equal(12, obj.Object.IntValue);
        }

        [Fact]
        public void WriteWithObject()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Object"":{""IntValue"":12}}";

            ObjectWithObject obj = new ObjectWithObject
            {
                Object = new IntObject
                {
                    IntValue = 12
                }
            };

            Helper.TestWrite(obj, json, options);
        }

        public class GenericObject<T>
        {
            public T Value { get; set; }
        }

        [Fact]
        public void ReadGenericObject()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Value"":1}";
            GenericObject<int> obj = Helper.Read<GenericObject<int>>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(1, obj.Value);
        }

        [Fact]
        public void WriteGenericObject()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Value"":1}";

            GenericObject<int> obj = new GenericObject<int>
            {
                Value = 1
            };

            Helper.TestWrite(obj, json, options);
        }

        public class ObjectWithJsonPropertyName
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
        }

        [Fact]
        public void ReadWithJsonPropertyName()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""id"":12}";
            ObjectWithJsonPropertyName obj = Helper.Read<ObjectWithJsonPropertyName>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(12, obj.Id);
        }

        [Fact]
        public void WriteWithJsonPropertyName()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""id"":12}";

            ObjectWithJsonPropertyName obj = new ObjectWithJsonPropertyName
            {
                Id = 12
            };

            Helper.TestWrite(obj, json, options);
        }

        public class CamelCaseJsonNamingPolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
                {
                    return name;
                }

                return char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
        }

        [JsonNamingPolicy(typeof(CamelCaseJsonNamingPolicy))]
        public class ObjectWithNamingPolicy
        {
            public int MyValue { get; set; }
        }

        [Fact]
        public void ReadWithNamingPolicy()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""myValue"":12}";
            ObjectWithNamingPolicy obj = Helper.Read<ObjectWithNamingPolicy>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(12, obj.MyValue);
        }

        public class ObjectWithCustomConverterOnProperty
        {
            [JsonConverter(typeof(GuidConverter))]
            public Guid Guid { get; set; }
        }

        [Fact]
        public void ReadWithCustomConverterOnProperty()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Guid"":""67ebf45d016c4b488ae61e389127b717""}";
            ObjectWithCustomConverterOnProperty obj = Helper.Read<ObjectWithCustomConverterOnProperty>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(Guid.Parse("67EBF45D-016C-4B48-8AE6-1E389127B717"), obj.Guid);
        }

        [Fact]
        public void WriteWithCustomConverterOnProperty()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Guid"":""67ebf45d016c4b488ae61e389127b717""}";
            ObjectWithCustomConverterOnProperty obj = new ObjectWithCustomConverterOnProperty
            {
                Guid = Guid.Parse("67EBF45D-016C-4B48-8AE6-1E389127B717")
            };
            Helper.TestWrite(obj, json, options);
        }

        public class JsonNodeObject
        {
            public JsonNode JsonNode { get; set; }
        }

        [Theory]
        [InlineData(@"{""JsonNode"":1}", JsonValueKind.Number)]
        [InlineData(@"{""JsonNode"":""foo""}", JsonValueKind.String)]
        [InlineData(@"{""JsonNode"":true}", JsonValueKind.True)]
        [InlineData(@"{""JsonNode"":false}", JsonValueKind.False)]
        [InlineData(@"{""JsonNode"":null}", JsonValueKind.Null)]
        [InlineData(@"{""JsonNode"":{""id"":1,""name"":""foo""}}", JsonValueKind.Object)]
        [InlineData(@"{""JsonNode"":[1,true,null]}", JsonValueKind.Array)]
        public void ReadWithJsonNode(string json, JsonValueKind expectedJsonValueKind)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            JsonNodeObject obj = Helper.Read<JsonNodeObject>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(expectedJsonValueKind, obj.JsonNode.ValueKind);
        }

        [Fact]
        public void WriteWithJsonNode()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""JsonNode"":[1,true,null]}";

            JsonNodeObject obj = new JsonNodeObject
            {
                JsonNode = new JsonArray(new JsonNode[] { 1, true, new JsonNull() })
            };
                
            Helper.TestWrite(obj, json, options);
        }
    }
}
