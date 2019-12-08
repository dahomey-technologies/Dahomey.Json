using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class BaseObjectHolder
    {
        public BaseObject BaseObject { get; set; }
        public NameObject NameObject { get; set; }
    }

    public class BaseObject
    {
        public int Id { get; set; }
    }

    [JsonDiscriminator(12)]
    public class NameObject : BaseObject
    {
        public string Name { get; set; }
    }

    public class DiscriminatorTests
    {
        [Fact]
        public void ReadPolymorphicObject()
        {
            const string json = @"{""BaseObject"":{""$type"":12,""Name"":""foo"",""Id"":1}}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.RegisterConvention(new AttributeBasedDiscriminatorConvention<int>(options));
            registry.RegisterType<NameObject>();

            BaseObjectHolder obj = JsonSerializer.Deserialize<BaseObjectHolder>(json, options);

            Assert.NotNull(obj);
            Assert.IsType<NameObject>(obj.BaseObject);
            Assert.Equal("foo", ((NameObject)obj.BaseObject).Name);
            Assert.Equal(1, obj.BaseObject.Id);
        }

        [Fact]
        public void ReadPolymorphicObjectWithDefaultDiscriminatorConvention()
        {
            const string json = @"{""$type"":""Dahomey.Json.Tests.BaseObjectHolder, Dahomey.Json.Tests"",""BaseObject"":{""$type"":12,""Name"":""foo"",""Id"":1}}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.RegisterConvention(new AttributeBasedDiscriminatorConvention<int>(options));
            registry.RegisterType<NameObject>();

            object obj = JsonSerializer.Deserialize<object>(json, options);

            Assert.NotNull(obj);
            Assert.IsType<BaseObjectHolder>(obj);
        }

        [Theory]
        [InlineData(DiscriminatorPolicy.Default, @"{""BaseObject"":{""$type"":12,""Name"":""foo"",""Id"":1},""NameObject"":{""Name"":""bar"",""Id"":2}}")]
        [InlineData(DiscriminatorPolicy.Auto, @"{""BaseObject"":{""$type"":12,""Name"":""foo"",""Id"":1},""NameObject"":{""Name"":""bar"",""Id"":2}}")]
        [InlineData(DiscriminatorPolicy.Never, @"{""BaseObject"":{""Name"":""foo"",""Id"":1},""NameObject"":{""Name"":""bar"",""Id"":2}}")]
        [InlineData(DiscriminatorPolicy.Always, @"{""$type"":""Dahomey.Json.Tests.BaseObjectHolder, Dahomey.Json.Tests"",""BaseObject"":{""$type"":12,""Name"":""foo"",""Id"":1},""NameObject"":{""$type"":12,""Name"":""bar"",""Id"":2}}")]
        public void WritePolymorphicObject(DiscriminatorPolicy discriminatorPolicy, string expected)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.RegisterConvention(new AttributeBasedDiscriminatorConvention<int>(options));
            registry.RegisterType<NameObject>();
            registry.DiscriminatorPolicy = discriminatorPolicy;

            BaseObjectHolder obj = new BaseObjectHolder
            {
                BaseObject = new NameObject
                {
                    Id = 1,
                    Name = "foo"
                },
                NameObject = new NameObject
                {
                    Id = 2,
                    Name = "bar"
                }
            };

            string actual = JsonSerializer.Serialize(obj, options);

            Assert.Equal(expected, actual);
        }

        private class CustomDiscriminatorConvention : IDiscriminatorConvention
        {
            private readonly ReadOnlyMemory<byte> _memberName = Encoding.ASCII.GetBytes("type");
            private readonly Dictionary<int, Type> _typesByDiscriminator = new Dictionary<int, Type>();
            private readonly Dictionary<Type, int> _discriminatorsByType = new Dictionary<Type, int>();

            public ReadOnlySpan<byte> MemberName => _memberName.Span;

            public bool TryRegisterType(Type type)
            {
                int discriminator = 17;
                foreach (char c in type.Name)
                {
                    discriminator = discriminator * 23 + (int)c;
                }

                _typesByDiscriminator.Add(discriminator, type);
                _discriminatorsByType.Add(type, discriminator);

                return true;
            }

            public Type ReadDiscriminator(ref Utf8JsonReader reader)
            {
                int discriminator = reader.GetInt32();
                if (!_typesByDiscriminator.TryGetValue(discriminator, out Type type))
                {
                    throw new JsonException($"Unknown type discriminator: {discriminator}");
                }
                return type;
            }

            public void WriteDiscriminator(Utf8JsonWriter writer, Type actualType)
            {
                if (!_discriminatorsByType.TryGetValue(actualType, out int discriminator))
                {
                    throw new JsonException($"Unknown discriminator for type: {actualType}");
                }

                writer.WriteNumberValue(discriminator);
            }
        }

        [Fact]
        public void ReadWithCustomDiscriminator()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.RegisterConvention(new CustomDiscriminatorConvention());
            registry.RegisterType(typeof(NameObject));

            const string json = @"{""BaseObject"":{""type"":571690115,""Name"":""foo"",""Id"":1}}";
            BaseObjectHolder obj = JsonSerializer.Deserialize<BaseObjectHolder>(json, options);

            Assert.NotNull(obj);
            Assert.IsType<NameObject>(obj.BaseObject);
            Assert.Equal("foo", ((NameObject)obj.BaseObject).Name);
            Assert.Equal(1, obj.BaseObject.Id);
        }

        [Fact]
        public void WriteWithCustomDiscriminator()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.RegisterConvention(new CustomDiscriminatorConvention());
            registry.RegisterType(typeof(NameObject));

            BaseObjectHolder obj = new BaseObjectHolder
            {
                BaseObject = new NameObject
                {
                    Id = 1,
                    Name = "foo"
                },
            };

            string actual = JsonSerializer.Serialize(obj, options);
            const string expected = @"{""BaseObject"":{""type"":571690115,""Name"":""foo"",""Id"":1},""NameObject"":null}";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadWithNoDiscriminator()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.ClearConventions();

            const string json = @"{""Name"":""foo"",""Id"":12}";
            BaseObject obj = JsonSerializer.Deserialize<BaseObject>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(12, obj.Id);
        }

        [Fact]
        public void WriteWithNoDiscriminator()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.ClearConventions();

            NameObject obj = new NameObject
            {
                Id = 12,
                Name = "foo"
            };

            string actual = JsonSerializer.Serialize<BaseObject>(obj, options);
            const string expected = @"{""Name"":""foo"",""Id"":12}";

            Assert.Equal(expected, actual);
        }
    }
}
