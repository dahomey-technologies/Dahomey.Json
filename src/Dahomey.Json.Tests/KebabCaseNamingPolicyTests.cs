using System.Text.Json;
using System.Text.Json.Serialization;
using Dahomey.Json.NamingPolicies;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class KebabCaseNamingPolicyTests
    {
        public class StringObject
        {
            [JsonPropertyName("AnotherName")] public string String { get; set; }
            
            public string PascalCaseProperty { get; set; }
            
            public string camelCaseProperty { get; set; }
            
            public string _0ALotOf1Symbols { get; set; }

            public string lowercasename { get; set; }

            public StringObject SetAll()
            {
                PascalCaseProperty = camelCaseProperty = _0ALotOf1Symbols = lowercasename = String = "test";
                return this;
            }
        }

        [Fact]
        public void TestConvertWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new KebabCaseNamingPolicy()
            }.SetupExtensions();
            
            var actual = Helper.Write(new StringObject().SetAll(), options);
            const string expected = @"{""AnotherName"":""test"",""pascal-case-property"":""test"",""camel-case-property"":""test"",""_0-a-lot-of1-symbols"":""test"",""lowercasename"":""test""}";
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void TestConvertRead()
        {
            const string json = @"{""AnotherName"":""test"",""pascal-case-property"":""test"",""camel-case-property"":""test"",""_0-a-lot-of1-symbols"":""test"",""lowercasename"":""test""}";
            
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new KebabCaseNamingPolicy()
            }.SetupExtensions();
            
            var actual = Helper.Read<StringObject>(json, options);

            string[] values =
            {
                actual.lowercasename, actual.String, actual.camelCaseProperty, actual.PascalCaseProperty,
                actual._0ALotOf1Symbols
            };
            Assert.All(values, value => Assert.Equal("test", value));
        }
    }
}