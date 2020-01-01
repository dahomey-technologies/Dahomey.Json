using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class DataContractAndMemberTests
    {
        [DataContract]
        public class ObjectWithCustomName
        {
            [DataMember(Name = "bar")]
            public int Foo { get; set; }
        }

        [Fact]
        public void ReadCustomName()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""bar"":12}";
            var obj = JsonSerializer.Deserialize<ObjectWithCustomName>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(12, obj.Foo);
        }

        [Fact]
        public void WriteCustomName()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""bar"":12}";
            var obj = new ObjectWithCustomName
            {
                Foo = 12
            };

            Helper.TestWrite(obj, json, options);
        }

        [DataContract]
        public class ObjectWithOptIn
        {
            [DataMember]
            public int OptIn { get; set; }
            public int OptOut { get; set; }
        }

        [Fact]
        public void ReadOptIn()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""OptIn"":12,""OptOut"":13}";
            var obj = JsonSerializer.Deserialize<ObjectWithOptIn>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(12, obj.OptIn);
            Assert.Equal(0, obj.OptOut);
        }

        [Fact]
        public void WriteOptIn()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""OptIn"":12}";
            var obj = new ObjectWithOptIn
            {
                OptIn = 12,
                OptOut = 13
            };

            Helper.TestWrite(obj, json, options);
        }

        [DataContract]
        public class ObjectWithRequiredProperty
        {
            [DataMember(IsRequired = true)]
            public string String { get; set; }
        }

        [Theory]
        [InlineData(@"{}", typeof(JsonException))]
        [InlineData(@"{""String"":null}", typeof(JsonException))]
        [InlineData(@"{""String"":""Foo""}", null)]
        public void ReadIsRequired(string json, Type expectedExceptionType)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestRead<ObjectWithRequiredProperty>(json, options, expectedExceptionType);
        }

        [Theory]
        [InlineData("", null)]
        [InlineData(null, typeof(JsonException))]
        [InlineData("Foo", null)]
        public void WriteIsRequired(string value, Type expectedExceptionType)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            var obj = new ObjectWithRequiredProperty
            {
                String = value
            };

            Helper.TestWrite(obj, options, expectedExceptionType);
        }

        [DataContract]
        public class ObjectWithDefaultValue
        {
            [DataMember(EmitDefaultValue = false)]
            [DefaultValue(12)]
            public int Id { get; set; }

            [DataMember(EmitDefaultValue = false)]
            [DefaultValue("foo")]
            public string FirstName { get; set; }

            [DataMember(EmitDefaultValue = false)]
            [DefaultValue("foo")]
            public string LastName { get; set; }

            [DataMember(EmitDefaultValue = false)]
            [DefaultValue(12)]
            public int Age { get; set; }
        }

        [Fact]
        public void WriteByAttribute()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            var obj = new ObjectWithDefaultValue
            {
                Id = 13,
                FirstName = "foo",
                LastName = "bar",
                Age = 12
            };

            const string json = @"{""Id"":13,""LastName"":""bar""}";

            Helper.TestWrite(obj, json, options);
        }
    }
}
