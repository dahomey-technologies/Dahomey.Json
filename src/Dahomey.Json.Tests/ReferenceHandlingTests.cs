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
    }
}
