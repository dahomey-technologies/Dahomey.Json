using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0029
    {
        private abstract class BaseClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [JsonDiscriminator(nameof(InheritedClass))]
        private class InheritedClass : BaseClass
        {
            [JsonConstructorEx]
            public InheritedClass(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        [Fact]
        public void Test()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.RegisterType<InheritedClass>();

            const string json = @"{""$type"":""InheritedClass"",""Id"":12,""Name"":""foo"",""Age"":13}";

            BaseClass obj = Helper.Read<BaseClass>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(12, obj.Id);
            Assert.Equal("foo", obj.Name);
            Assert.Equal(13, obj.Age);
        }
    }
}
