using Dahomey.Json.Serialization.Converters.Mappings;
using System;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.StackOverflow
{
    /// <summary>
    /// https://stackoverflow.com/questions/58926112/system-text-json-api-is-there-something-like-icontractresolver
    /// </summary>
    public class Question58926112
    {
        public class SelectiveSerializer : IObjectMappingConvention
        {
            private readonly IObjectMappingConvention defaultObjectMappingConvention = new DefaultObjectMappingConvention();
            private readonly string[] fields;

            public SelectiveSerializer(string fields)
            {
                var fieldColl = fields.Split(',');
                this.fields = fieldColl
                    .Select(f => f.ToLower().Trim())
                    .ToArray();
            }

            public void Apply<T>(JsonSerializerOptions options, ObjectMapping<T> objectMapping)
            {
                defaultObjectMappingConvention.Apply<T>(options, objectMapping);
                foreach (IMemberMapping memberMapping in objectMapping.MemberMappings)
                {
                    if (memberMapping is MemberMapping<T> member)
                    {
                        member.SetShouldSerializeMethod(o => fields.Contains(member.MemberName.ToLower()));
                    }
                }
            }
        }

        public class Employee
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
        }

        [Fact]
        public void Test()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingConventionRegistry().RegisterConvention(
                typeof(Employee), new SelectiveSerializer("FirstName,Email,Id"));

            Employee employee = new Employee
            {
                Id = 12,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@acme.com"
            };

            const string expected = @"{""Id"":12,""FirstName"":""John"",""Email"":""john.doe@acme.com""}";
            string actual = JsonSerializer.Serialize(employee, options);

            Assert.Equal(expected, actual);
        }
    }
}
