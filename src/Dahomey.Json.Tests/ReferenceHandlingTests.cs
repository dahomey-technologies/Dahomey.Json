using Dahomey.Json.Attributes;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class ReferenceHandlingTests
    {
        public class Employee
        {
            [JsonIgnoreIfDefault]
            public string Name { get; set; }

            [JsonIgnoreIfDefault]
            public Employee Manager { get; set; }

            [JsonIgnoreIfDefault]
            public List<Employee> Subordinates { get; set; }
        }

        [Fact]
        public void WriteIgnoringReferenceLoops()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetReferenceHandling(ReferenceHandling.Ignore);

            var bob = new Employee { Name = "Bob" };
            var angela = new Employee { Name = "Angela" };

            angela.Manager = bob;
            bob.Subordinates = new List<Employee> { angela };

            const string json = @"{""Name"":""Angela"",""Manager"":{""Name"":""Bob"",""Subordinates"":[]}}";
            Helper.TestWrite(angela, json, options);
        }

        [Fact]
        public void WritePreservingReference()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetReferenceHandling(ReferenceHandling.Preserve);

            var bob = new Employee { Name = "Bob" };
            var angela = new Employee { Name = "Angela" };

            angela.Manager = bob;
            bob.Subordinates = new List<Employee> { angela };

            const string json = @"{""$id"":""1"",""Name"":""Angela"",""Manager"":{""$id"":""2"",""Name"":""Bob"",""Subordinates"":{""$id"":""3"",""$values"":[{""$ref"":""1""}]}}}";
            Helper.TestWrite(angela, json, options);
        }

        [Fact]
        public void ReadPreservingReference()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetReferenceHandling(ReferenceHandling.Preserve);

            const string json = @"{""$id"":""1"",""Name"":""Angela"",""Manager"":{""$id"":""2"",""Name"":""Bob"",""Subordinates"":{""$id"":""3"",""$values"":[{""$ref"":""1""}]}}}";
            var angela = JsonSerializer.Deserialize<Employee>(json, options);

            Assert.NotNull(angela);
            Assert.Equal("Angela", angela.Name);
            Assert.NotNull(angela.Manager);
            Assert.Equal("Bob", angela.Manager.Name);
            Assert.NotNull(angela.Manager.Subordinates);
            Assert.Single(angela.Manager.Subordinates);
            Assert.Same(angela, angela.Manager.Subordinates[0]);
        }
    }
}
