using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class MissingMemberHandlingTests
    {
        public class Account
        {
            public string FullName { get; set; }
            public bool Deleted { get; set; }
        }

        public class Base
        {
            public int Int { get; set; }
        }

        [JsonDiscriminator("Derived")]
        public class Derived : Base
        {
            public string String { get; set; }
        }

        [Fact]
        public void TestMissingMemberHandlingIgnore()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetMissingMemberHandling(MissingMemberHandling.Ignore);

            const string json = @"{""FullName"":""Dan Deleted"",""Deleted"":true,""DeletedDate"":""2013-01-20T00:00:00""}";
            var obj = JsonSerializer.Deserialize<Account>(json, options);
            Assert.NotNull(obj);
            Assert.Equal("Dan Deleted", obj.FullName);
            Assert.True(obj.Deleted);
        }

        [Fact]
        public void TestMissingMemberHandlingError()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetMissingMemberHandling(MissingMemberHandling.Error);

            const string json = @"{""FullName"":""Dan Deleted"",""Deleted"":true,""DeletedDate"":""2013-01-20T00:00:00""}";
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Account>(json, options));
        }

        [Fact]
        public void TestPolymorphicMissingMemberHandlingIgnore()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetMissingMemberHandling(MissingMemberHandling.Ignore);

            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.ClearConventions();
            registry.RegisterConvention(new DefaultDiscriminatorConvention<string>(options, "Type"));
            registry.RegisterType<Derived>();

            const string json = @"{""Type"":""Derived"",""Int"":42,""String"":""xyz"",""Missing"":0}";
            var obj = JsonSerializer.Deserialize<Base>(json, options);
            Assert.NotNull(obj);
            Assert.IsType<Derived>(obj);
            Assert.Equal(42, obj.Int);
            Assert.Equal("xyz", (obj as Derived).String);
        }

        [Fact]
        public void TestPolymorphicMissingMemberHandlingError()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetMissingMemberHandling(MissingMemberHandling.Error);

            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.ClearConventions();
            registry.RegisterConvention(new DefaultDiscriminatorConvention<string>(options, "Type"));
            registry.RegisterType<Derived>();

            const string json = @"{""Type"":""Derived"",""Int"":42,""String"":""xyz""}";
            var obj = JsonSerializer.Deserialize<Base>(json, options);
            Assert.NotNull(obj);
            Assert.IsType<Derived>(obj);
            Assert.Equal(42, obj.Int);
            Assert.Equal("xyz", (obj as Derived).String);

            const string json2 = @"{""Type"":""Derived"",""Int"":42,""String"":""xyz"",""Missing"":0}";
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Base>(json2, options));
        }
    }
}
