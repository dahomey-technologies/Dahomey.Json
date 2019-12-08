using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class ShouldSerializeTests
    {
        private class ObjectWithShouldSerialize
        {
            public int Id { get; set; }

            public bool ShouldSerializeId()
            {
                return Id != 12;
            }
        }

        [Theory]
        [InlineData(12, "{}")]
        [InlineData(13, @"{""Id"":13}")]
        public void TestAutomaticMethod(int id, string json)
        {
            ObjectWithShouldSerialize obj = new ObjectWithShouldSerialize
            {
                Id = id
            };

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestWrite(obj, json, options);
        }

        private class ObjectWithShouldSerialize2
        {
            public int Id { get; set; }
        }

        [Theory]
        [InlineData(12, "{}")]
        [InlineData(13, @"{""Id"":13}")]
        public void TestCustomMethod(int id, string json)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<ObjectWithShouldSerialize2>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .ClearMemberMappings()
                    .MapMember(o => o.Id)
                        .SetShouldSerializeMethod(o => ((ObjectWithShouldSerialize2)o).Id != 12)
            );

            ObjectWithShouldSerialize2 obj = new ObjectWithShouldSerialize2
            {
                Id = id
            };

            Helper.TestWrite(obj, json, options);
        }
    }
}
