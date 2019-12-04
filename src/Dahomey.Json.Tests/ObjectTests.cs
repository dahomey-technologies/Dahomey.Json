using System;
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
            public DateTime DateTime { get; set; }
            public EnumTest Enum { get; set; }
        }

        [Fact]
        public void ReadSimpleObject()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.Converters.Add(new JsonStringEnumConverter());

            const string json = @"{""Boolean"":true,""SByte"":13,""Byte"":12,""Int16"":14,""UInt16"":15,""Int32"":16,""UInt32"":17,""Int64"":18,""UInt64"":19,""String"":""string"",""Single"":20.209999084472656,""Double"":22.23,""DateTime"":""2014-02-21T19:00:00Z"",""Enum"":""Value1""}";
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
                Single = 20.21f,
                Double = 22.23,
                String = "string",
                DateTime = new DateTime(2014, 02, 21, 19, 0, 0, DateTimeKind.Utc),
                Enum = EnumTest.Value1
            };

            const string expected = @"{""Boolean"":true,""SByte"":13,""Byte"":12,""Int16"":14,""UInt16"":15,""Int32"":16,""UInt32"":17,""Int64"":18,""UInt64"":19,""String"":""string"",""Single"":20.2099991,""Double"":22.23,""DateTime"":""2014-02-21T19:00:00Z"",""Enum"":""Value1""}";
            string actual = JsonSerializer.Serialize(obj, options);

            Assert.Equal(expected, actual);
        }
    }
}
