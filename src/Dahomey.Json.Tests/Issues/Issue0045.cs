using Dahomey.Json.Attributes;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0045
    {
        public readonly struct MyStruct
        {
            [JsonConstructor("Value")]
            public MyStruct(int value)
            {
                Value = value;
            }

            public int Value { get; }
        }

        [Fact]
        public void Test()
        {
            var s1 = new MyStruct(100);
            var options = new JsonSerializerOptions();
            options.SetupExtensions();
            var json = JsonSerializer.Serialize(s1, options);
            var s2 = JsonSerializer.Deserialize<MyStruct>(json, options);
        }
    }
}
