using Xunit;
using System.ComponentModel;
using Dahomey.Json.Attributes;
using System.Text.Json;

namespace Dahomey.Json.Tests
{
    public class DefaultValueTests
    {
        public class ObjectWithDefaultValue
        {
            [JsonIgnoreIfDefault]
            [DefaultValue(12)]
            public int Id { get; set; }

            [JsonIgnoreIfDefault]
            [DefaultValue("foo")]
            public string FirstName { get; set; }

            [JsonIgnoreIfDefault]
            [DefaultValue("foo")]
            public string LastName { get; set; }

            [JsonIgnoreIfDefault]
            [DefaultValue(12)]
            public int Age { get; set; }
        }

        [Fact]
        public void WriteByAttribute()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            ObjectWithDefaultValue obj = new ObjectWithDefaultValue
            {
                Id = 13,
                FirstName = "foo",
                LastName = "bar",
                Age = 12
            };

            const string json = @"{""Id"":13,""LastName"":""bar""}";

            Helper.TestWrite(obj, json, options);
        }

        public class ObjectWithDefaultValue2
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Age { get; set; }
        }

        [Fact]
        public void WriteValueByApi()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<ObjectWithDefaultValue2>(objectMapping =>
            {
                objectMapping.AutoMap();
                objectMapping.ClearMemberMappings();
                objectMapping.MapMember(o => o.Id).SetDefaultValue(12).SetIgnoreIfDefault(true);
                objectMapping.MapMember(o => o.FirstName).SetDefaultValue("foo").SetIgnoreIfDefault(true);
                objectMapping.MapMember(o => o.LastName).SetDefaultValue("foo").SetIgnoreIfDefault(true);
                objectMapping.MapMember(o => o.Age).SetDefaultValue(12).SetIgnoreIfDefault(true);
            });

            ObjectWithDefaultValue2 obj = new ObjectWithDefaultValue2
            {
                Id = 13,
                FirstName = "foo",
                LastName = "bar",
                Age = 12
            };

            const string json = @"{""Id"":13,""LastName"":""bar""}";

            Helper.TestWrite(obj, json, options);
        }
    }
}
