using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0053
    {
        public class MyClass
        {
            public ReadOnlyDictionary<string, int> Dict { get; set; }
        }

        [Fact]
        public void Test()
        {
            MyClass myClass = new MyClass
            {
                Dict = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>
                {
                    ["foo"] = 1,
                    ["bar"] = 2
                })
            };

            const string expectedJson = @"{""Dict"":{""foo"":1,""bar"":2}}";

            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            string actualJson = JsonSerializer.Serialize(myClass, options);
            Assert.Equal(expectedJson, actualJson);
        }
    }
}
