using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Serialization.Converters.Mappings;
using System;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0039
    {
        public class DiscriminatorObjectMappingConventionProvider : IObjectMappingConventionProvider
        {
            private readonly Func<Type, bool> _typeFilter;

            public DiscriminatorObjectMappingConventionProvider(Func<Type, bool> typeFilter)
            {
                _typeFilter = typeFilter;
            }

            public IObjectMappingConvention GetConvention(Type type)
            {
                if (_typeFilter(type))
                {
                    return new DiscriminatorObjectMappingConvention();
                }
                else
                {
                    return null;
                }
            }
        }

        public class DiscriminatorObjectMappingConvention : IObjectMappingConvention
        {
            private readonly DefaultObjectMappingConvention _defaultConvention = new DefaultObjectMappingConvention();

            public void Apply<T>(JsonSerializerOptions options, ObjectMapping<T> objectMapping)
            {
                _defaultConvention.Apply(options, objectMapping);

               objectMapping.SetDiscriminator(typeof(T).Name);
            }
        }

        public class Foo
        {
            public int Id { get; set; }
        }

        public class Bar
        {
            public int Id { get; set; }
        }

        [Fact]
        public void Read()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingConventionRegistry().RegisterProvider(
                new DiscriminatorObjectMappingConventionProvider(t => t == typeof(Foo))
            );
            options.GetDiscriminatorConventionRegistry().DiscriminatorPolicy = DiscriminatorPolicy.Always;

            Assert.Equal(@"{""$type"":""Foo"",""Id"":1}", JsonSerializer.Serialize(new Foo { Id = 1 }, options));
            Assert.Equal(@"{""Id"":1}", JsonSerializer.Serialize(new Bar { Id = 1 }, options));
        }
    }
}
