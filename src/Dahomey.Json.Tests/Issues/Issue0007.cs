using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0007
    {
        public class Test
        {
            public Test Parent { get; set; }
            public List<Test> Children { get; set; }
        }

        [Fact]
        public void TestRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            }.SetupExtensions();

            const string json = @"{""Parent"":{},""Children"":[{},{}]}";
            var test = JsonSerializer.Deserialize<Test>(json, options);

            Assert.NotNull(test);
            Assert.NotNull(test.Parent);
            Assert.NotNull(test.Children);
            Assert.Equal(2, test.Children.Count);
            Assert.NotNull(test.Children[0]);
            Assert.NotNull(test.Children[1]);
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            }.SetupExtensions();

            const string json = @"{""Parent"":{},""Children"":[{},{}]}";
            var test = new Test
            {
                Parent = new Test(),
                Children = new List<Test> { new Test(), new Test() }
            };

            Helper.TestWrite(test, json, options);
        }
    }
}
