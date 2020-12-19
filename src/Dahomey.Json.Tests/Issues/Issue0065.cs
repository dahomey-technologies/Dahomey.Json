using System;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0065
    {
        public class Test
        {
            public Func<int> onValueChanged { get; set; } // also tried as a field with same result
        }

        [Fact]
        public void TestFuncProperty()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            var test = JsonSerializer.Deserialize<Test>(@"{}", options);
        }
    }
}
