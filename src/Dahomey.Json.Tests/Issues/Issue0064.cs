using Dahomey.Json.Serialization.Converters.Mappings;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0064
    {
        public class CustomObjectMappingConventionProvider : IObjectMappingConventionProvider
        {
            public Dictionary<string, string> FieldNameChanges { get; set; }

            public IObjectMappingConvention GetConvention(Type type)
            {
                // here you could filter which type should be handled by the custom convention and return null for other types
                return new CustomObjectMappingConvention(FieldNameChanges);
            }
        }

        public class CustomObjectMappingConvention : IObjectMappingConvention
        {
            private readonly Dictionary<string, string> _fieldNameChanges;

            private readonly DefaultObjectMappingConvention _defaultConvention = new DefaultObjectMappingConvention();

            public CustomObjectMappingConvention(Dictionary<string, string> fieldNameChanges)
            {
                _fieldNameChanges = fieldNameChanges;
            }

            public void Apply<T>(JsonSerializerOptions options, ObjectMapping<T> objectMapping)
            {
                _defaultConvention.Apply(options, objectMapping);

                foreach (IMemberMapping memberMapping in objectMapping.MemberMappings)
                {
                    if (_fieldNameChanges.Count > 0 && _fieldNameChanges.TryGetValue(memberMapping.MemberInfo.Name, out string newName))
                    {
                        memberMapping.MemberName = newName;
                    }
                }
            }
        }

        public class Object
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void Test()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingConventionRegistry().RegisterProvider(
                new CustomObjectMappingConventionProvider
                { 
                    FieldNameChanges = new Dictionary<string, string>
                    { 
                        ["Name"] = "foo"
                    }
                }
            );

            const string json = @"{""foo"":""bar"",""Id"":1}";
            Object obj = Helper.Read<Object>(json, options);

            Assert.NotNull(obj);
            Assert.Equal("bar", obj.Name);
            Assert.Equal(1, obj.Id);
        }
    }
}
