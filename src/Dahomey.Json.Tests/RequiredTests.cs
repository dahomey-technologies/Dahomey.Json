using Dahomey.Json.Attributes;
using System;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class RequiredTests
    {
        public class StringObject
        {
            public string String { get; set; }
        }

        [Theory]
        [InlineData(RequirementPolicy.Never, @"{}", null)]
        [InlineData(RequirementPolicy.Never, @"{""String"":null}", null)]
        [InlineData(RequirementPolicy.Never, @"{""String"":""Foo""}", null)]
        [InlineData(RequirementPolicy.Always, @"{}", typeof(JsonException))]
        [InlineData(RequirementPolicy.Always, @"{""String"":null}", typeof(JsonException))]
        [InlineData(RequirementPolicy.Always, @"{""String"":""Foo""}", null)]
        [InlineData(RequirementPolicy.AllowNull, @"{}", typeof(JsonException))]
        [InlineData(RequirementPolicy.AllowNull, @"{""String"":null}", null)]
        [InlineData(RequirementPolicy.AllowNull, @"{""String"":""Foo""}", null)]
        [InlineData(RequirementPolicy.DisallowNull, @"{}", null)]
        [InlineData(RequirementPolicy.DisallowNull, @"{""String"":null}", typeof(JsonException))]
        [InlineData(RequirementPolicy.DisallowNull, @"{""String"":""Foo""}", null)]
        public void TestRead(RequirementPolicy requirementPolicy, string json, Type expectedExceptionType)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<StringObject>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .ClearMemberMappings()
                    .MapMember(o => o.String).SetRequired(requirementPolicy)
            );

            Helper.TestRead<StringObject>(json, options, expectedExceptionType);
        }

        [Theory]
        [InlineData(RequirementPolicy.Never, "", null)]
        [InlineData(RequirementPolicy.Never, null, null)]
        [InlineData(RequirementPolicy.Never, "Foo", null)]
        [InlineData(RequirementPolicy.Always, "", null)]
        [InlineData(RequirementPolicy.Always, null, typeof(JsonException))]
        [InlineData(RequirementPolicy.Always, "Foo", null)]
        [InlineData(RequirementPolicy.AllowNull, "", null)]
        [InlineData(RequirementPolicy.AllowNull, null, null)]
        [InlineData(RequirementPolicy.AllowNull, "Foo", null)]
        [InlineData(RequirementPolicy.DisallowNull, "", null)]
        [InlineData(RequirementPolicy.DisallowNull, null, typeof(JsonException))]
        [InlineData(RequirementPolicy.DisallowNull, "Foo", null)]
        public void TestWrite(RequirementPolicy requirementPolicy, string value, Type expectedExceptionType)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<StringObject>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .ClearMemberMappings()
                    .MapMember(o => o.String).SetRequired(requirementPolicy)
            );

            StringObject obj = new StringObject
            {
                String = value
            };

            Helper.TestWrite(obj, options, expectedExceptionType);
        }

        public class StringObjectWithAttribute
        {
            [JsonRequired]
            public string String { get; set; }
        }

        [Theory]
        [InlineData(@"{}", typeof(JsonException))]
        [InlineData(@"{""String"":null}", typeof(JsonException))]
        [InlineData(@"{""String"":""Foo""}", null)]
        public void TestReadWithAttribute(string json, Type expectedExceptionType)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestRead<StringObjectWithAttribute>(json, options, expectedExceptionType);
        }

        [Theory]
        [InlineData("", null)]
        [InlineData(null, typeof(JsonException))]
        [InlineData("Foo", null)]
        public void TestWriteWithAttribute(string value, Type expectedExceptionType)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            StringObjectWithAttribute obj = new StringObjectWithAttribute
            {
                String = value
            };

            Helper.TestWrite(obj, options, expectedExceptionType);
        }
    }
}
