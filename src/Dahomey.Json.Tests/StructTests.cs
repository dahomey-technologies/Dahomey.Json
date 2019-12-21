using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class StructTests
    {
        public struct FieldSerializationStruct
        {
            public int A { get; set; }
            public int B;

            public FieldSerializationStruct(int a, int b)
            {
                A = a;
                B = b;
            }
        }

        [Fact]
        public void ReadStruct()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""A"":12,""B"":13}";
            FieldSerializationStruct strct = JsonSerializer.Deserialize<FieldSerializationStruct>(json, options);

            Assert.Equal(12, strct.A);
            Assert.Equal(13, strct.B);
        }

        [Fact]
        public void WriteStruct()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""A"":12,""B"":13}";
            FieldSerializationStruct strct = new FieldSerializationStruct
            {
                A = 12,
                B = 13
            };

            Helper.TestWrite(strct, json, options);
        }

        public class FieldSerializationClassWithStruct
        {
            public int A { get; set; } = 1;
            public FieldSerializationStruct B = new FieldSerializationStruct(2, 3);
        }

        [Fact]
        public void ReadNestedStruct()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""A"":11,""B"":{""A"":12,""B"":13}}";
            FieldSerializationClassWithStruct strct = JsonSerializer.Deserialize<FieldSerializationClassWithStruct>(json, options);

            Assert.Equal(11, strct.A);
            Assert.Equal(12, strct.B.A);
            Assert.Equal(13, strct.B.B);
        }

        [Fact]
        public void WriteNestedStruct()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""A"":1,""B"":{""A"":2,""B"":3}}";
            FieldSerializationClassWithStruct strct = new FieldSerializationClassWithStruct();

            Helper.TestWrite(strct, json, options);
        }
    }
}
