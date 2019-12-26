using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class InterfaceCollectionTests
    {
        private class ObjectWithICollection
        {
            public ICollection<int> Collection { get; set; }
        }

        [Fact]
        public void ICollection()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Collection"":[12,13]}";
            ObjectWithICollection obj = JsonSerializer.Deserialize<ObjectWithICollection>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Collection);
            Assert.IsType<List<int>>(obj.Collection);
            Assert.Equal(2, obj.Collection.Count);
            Assert.Equal(12, obj.Collection.ElementAt(0));
            Assert.Equal(13, obj.Collection.ElementAt(1));

            string actual = JsonSerializer.Serialize(obj, obj.GetType(), options);
            Assert.Equal(json, actual);
        }

        private class ObjectWithISet
        {
            public ISet<int> Collection { get; set; }
        }

        [Fact]
        public void ISet()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Collection"":[12,13]}";
            ObjectWithISet obj = JsonSerializer.Deserialize<ObjectWithISet>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Collection);
            Assert.IsType<HashSet<int>>(obj.Collection);
            Assert.Equal(2, obj.Collection.Count);
            Assert.Equal(12, obj.Collection.ElementAt(0));
            Assert.Equal(13, obj.Collection.ElementAt(1));

            string actual = JsonSerializer.Serialize(obj, obj.GetType(), options);
            Assert.Equal(json, actual);
        }

        private class ObjectWithIDictionary
        {
            public IDictionary<int, int> Collection { get; set; }
        }

        [Fact]
        public void IDictionary()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Collection"":{""12"":12,""13"":13}}";
            ObjectWithIDictionary obj = JsonSerializer.Deserialize<ObjectWithIDictionary>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Collection);
            Assert.IsType<Dictionary<int, int>>(obj.Collection);
            Assert.Equal(2, obj.Collection.Count);
            Assert.Equal(12, obj.Collection[12]);
            Assert.Equal(13, obj.Collection[13]);

            string actual = JsonSerializer.Serialize(obj, obj.GetType(), options);
            Assert.Equal(json, actual);
        }
    }
}
