using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class BaseBaseObjectHolder
    {

    }

    public class BaseObjectHolder : BaseBaseObjectHolder
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

    [JsonDiscriminator(13)]
    public class OtherObject
    {
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
            registry.ClearConventions();
            registry.RegisterConvention(new DefaultDiscriminatorConvention<int>(options));
            registry.RegisterType<NameObject>();

            BaseObjectHolder obj = JsonSerializer.Deserialize<BaseObjectHolder>(json, options);

            Assert.NotNull(obj);
            Assert.IsType<NameObject>(obj.BaseObject);
            Assert.Equal("foo", ((NameObject)obj.BaseObject).Name);
            Assert.Equal(1, obj.BaseObject.Id);
        }

        [Fact]
        public void ReadNonAssignablePolymorphicObject()
        {
            const string json = @"{""BaseObject"":{""$type"":13}}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.ClearConventions();
            registry.RegisterConvention(new DefaultDiscriminatorConvention<int>(options));
            registry.RegisterType<NameObject>();
            registry.RegisterType<OtherObject>();

            Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseObjectHolder>(json, options));
        }

        [Theory]
        [InlineData(DiscriminatorPolicy.Default, @"{""BaseObject"":{""$type"":12,""Name"":""foo"",""Id"":1},""NameObject"":{""Name"":""bar"",""Id"":2}}")]
        [InlineData(DiscriminatorPolicy.Auto, @"{""BaseObject"":{""$type"":12,""Name"":""foo"",""Id"":1},""NameObject"":{""Name"":""bar"",""Id"":2}}")]
        [InlineData(DiscriminatorPolicy.Never, @"{""BaseObject"":{""Name"":""foo"",""Id"":1},""NameObject"":{""Name"":""bar"",""Id"":2}}")]
        [InlineData(DiscriminatorPolicy.Always, @"{""BaseObject"":{""$type"":12,""Name"":""foo"",""Id"":1},""NameObject"":{""$type"":12,""Name"":""bar"",""Id"":2}}")]
        public void WritePolymorphicObject(DiscriminatorPolicy discriminatorPolicy, string expected)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.ClearConventions();
            registry.RegisterConvention(new DefaultDiscriminatorConvention<int>(options));
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

        public class BaseObjectHolder2 : BaseBaseObjectHolder
        {
            public BaseObject2 BaseObject { get; set; }
            public NameObject2 NameObject { get; set; }
        }

        public class BaseObject2
        {
            public int Id { get; set; }
        }

        public class NameObject2 : BaseObject2
        {
            public string Name { get; set; }
        }

        [Fact]
        public void ReadWithCustomDiscriminator()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.RegisterConvention(new CustomDiscriminatorConvention());
            registry.RegisterType(typeof(NameObject2));

            const string json = @"{""BaseObject"":{""type"":263970807,""Name"":""foo"",""Id"":1}}";
            BaseObjectHolder2 obj = JsonSerializer.Deserialize<BaseObjectHolder2>(json, options);

            Assert.NotNull(obj);
            Assert.IsType<NameObject2>(obj.BaseObject);
            Assert.Equal("foo", ((NameObject2)obj.BaseObject).Name);
            Assert.Equal(1, obj.BaseObject.Id);
        }

        [Fact]
        public void WriteWithCustomDiscriminator()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.RegisterConvention(new CustomDiscriminatorConvention());
            registry.RegisterType(typeof(NameObject2));

            BaseObjectHolder2 obj = new BaseObjectHolder2
            {
                BaseObject = new NameObject2
                {
                    Id = 1,
                    Name = "foo"
                },
            };

            string actual = JsonSerializer.Serialize(obj, options);
            const string expected = @"{""BaseObject"":{""type"":263970807,""Name"":""foo"",""Id"":1},""NameObject"":null}";

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
        public void WriteWithNoDiscriminatorConvention()
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

        [Fact]
        public void ReadPolymorphicObjectWithDeferredTypeProperty()
        {
            const string json = @"{""BaseObject"":{""Name"":""foo"",""Id"":1,""Unknown"":{""Prop1"":12,""Prop2"":""bar""},""$type"":12}}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.ClearConventions();
            registry.RegisterConvention(new DefaultDiscriminatorConvention<int>(options));
            registry.RegisterType<NameObject>();

            BaseObjectHolder obj = JsonSerializer.Deserialize<BaseObjectHolder>(json, options);

            Assert.NotNull(obj);
            NameObject nameObject = Assert.IsType<NameObject>(obj.BaseObject);
            Assert.Equal("foo", nameObject.Name);
            Assert.Equal(1, nameObject.Id);
        }
    }
}
