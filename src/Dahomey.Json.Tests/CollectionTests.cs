using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class CollectionTests
    {
        public class IntObject
        {
            public int IntValue { get; set; }

            protected bool Equals(IntObject other)
            {
                return IntValue == other.IntValue;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((IntObject)obj);
            }

            public override int GetHashCode()
            {
                return IntValue;
            }
        }

        public class ObjectWithList
        {
            public List<int> IntList { get; set; }
            public List<IntObject> ObjectList { get; set; }
            public List<string> StringList { get; set; }
        }

        [Fact]
        public void ReadObjectWithList()
        {
            const string json = @"{""IntList"":[1,2],""ObjectList"":[{""IntValue"":1},{""IntValue"":2}],""StringList"":[""a"",""b""]}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            ObjectWithList obj = JsonSerializer.Deserialize<ObjectWithList>(json, options);

            Assert.NotNull(obj);

            Assert.NotNull(obj.IntList);
            Assert.Equal(2, obj.IntList.Count);
            Assert.Equal(1, obj.IntList[0]);
            Assert.Equal(2, obj.IntList[1]);

            Assert.NotNull(obj.ObjectList);
            Assert.Equal(2, obj.ObjectList.Count);
            Assert.NotNull(obj.ObjectList[0]);
            Assert.Equal(1, obj.ObjectList[0].IntValue);
            Assert.NotNull(obj.ObjectList[1]);
            Assert.Equal(2, obj.ObjectList[1].IntValue);

            Assert.NotNull(obj.StringList);
            Assert.Equal(2, obj.StringList.Count);
            Assert.Equal("a", obj.StringList[0]);
            Assert.Equal("b", obj.StringList[1]);
        }

        [Fact]
        public void WriteObjectWithList()
        {
            const string expected = @"{""IntList"":[1,2],""ObjectList"":[{""IntValue"":1},{""IntValue"":2}],""StringList"":[""a"",""b""]}";

            ObjectWithList obj = new ObjectWithList
            {
                IntList = new List<int> { 1, 2 },
                ObjectList = new List<IntObject>
                {
                    new IntObject { IntValue = 1 },
                    new IntObject { IntValue = 2 }
                },
                StringList = new List<string> { "a", "b" }
            };

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            string actual = JsonSerializer.Serialize(obj, obj.GetType(), options);

            Assert.Equal(expected, actual);
        }
    }
}
