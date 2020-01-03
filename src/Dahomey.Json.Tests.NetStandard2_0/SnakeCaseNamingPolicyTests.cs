using System.Text.Json;
using System.Text.Json.Serialization;
using Dahomey.Json.NamingPolicies;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class SnakeCaseNamingPolicyTests
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
                PropertyNamingPolicy = new SnakeCaseNamingPolicy()
            }.SetupExtensions();
            
            var actual = Helper.Write(new StringObject().SetAll(), options);
            const string expected = @"{""AnotherName"":""test"",""pascal_case_property"":""test"",""camel_case_property"":""test"",""_0_a_lot_of1_symbols"":""test"",""lowercasename"":""test""}";
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void TestConvertRead()
        {
            const string json = @"{""AnotherName"":""test"",""pascal_case_property"":""test"",""camel_case_property"":""test"",""_0_a_lot_of1_symbols"":""test"",""lowercasename"":""test""}";
            
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy()
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