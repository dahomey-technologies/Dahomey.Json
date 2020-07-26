using Dahomey.Json.Attributes;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0045
    {
        public readonly struct MyStruct
        {
            [JsonConstructorEx("IntValue")]
            public MyStruct(int intValue)
            {
                IntValue = intValue;
            }

            public int IntValue { get; }
        }

        [Fact]
        public void Test()
        {
            var s1 = new MyStruct(100);
            var options = new JsonSerializerOptions();
            options.SetupExtensions();
            var json = JsonSerializer.Serialize(s1, options);
            var s2 = JsonSerializer.Deserialize<MyStruct>(json, options);
            Assert.Equal(100, s2.IntValue);
        }

        [Fact]
        public void TestCamelCase()
        {
            var s1 = new MyStruct(100);
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            options.SetupExtensions();
            var json = JsonSerializer.Serialize(s1, options);
            var s2 = JsonSerializer.Deserialize<MyStruct>(json, options);
            // serialized as "intValue", but JsonConstructor specifies "IntValue"
            Assert.Equal(default, s2.IntValue);
        }
    }
}
