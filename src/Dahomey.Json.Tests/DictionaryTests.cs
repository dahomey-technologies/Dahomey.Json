using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class Dictionary
    {
        [Fact]
        public void ReadDictionary()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""1"":""foo"",""2"":""bar""}";
            Dictionary<int, string> actual = JsonSerializer.Deserialize<Dictionary<int, string>>(json, options);

            Dictionary<int, string> expected = new Dictionary<int, string>
            {
                [1] = "foo",
                [2] = "bar"
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteDictionary()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Dictionary<int, string> dictionary = new Dictionary<int, string>
            {
                [1] = "foo",
                [2] = "bar"
            };

            string actual = JsonSerializer.Serialize(dictionary, options);
            const string expected = @"{""1"":""foo"",""2"":""bar""}";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteDictionaryWithPolicy()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
            };
            options.SetupExtensions();

            Dictionary<string, string> dictionary = new Dictionary<string, string>
            {
                ["Foo"] = "foo",
                ["Bar"] = "bar"
            };

            string actual = JsonSerializer.Serialize(dictionary, options);
            const string expected = @"{""foo"":""foo"",""bar"":""bar""}";

            Assert.Equal(expected, actual);
        }

        public class ObjectWithDictionary
        {
            public Dictionary<int, SimpleObject> Dictionary { get; set; }
        }

        public class SimpleObject : IEquatable<SimpleObject>
        {
            public int Id { get; set; }

            public bool Equals(SimpleObject other)
            {
                return Id.Equals(other.Id);
            }
        }

        [Fact]
        public void ReadObjectWithDictionary()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Dictionary"":{""1"":{""Id"":1},""2"":{""Id"":2}}}";
            ObjectWithDictionary actual = JsonSerializer.Deserialize<ObjectWithDictionary>(json, options);

            ObjectWithDictionary expected = new ObjectWithDictionary
            {
                Dictionary = new Dictionary<int, SimpleObject>
                {
                    [1] = new SimpleObject { Id = 1 },
                    [2] = new SimpleObject { Id = 2 }
                }
            };

            Assert.Equal(expected.Dictionary, actual.Dictionary);
        }

        [Fact]
        public void WriteObjectWithDictionary()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            ObjectWithDictionary obj = new ObjectWithDictionary
            {
                Dictionary = new Dictionary<int, SimpleObject>
                {
                    [1] = new SimpleObject { Id = 1 },
                    [2] = new SimpleObject { Id = 2 }
                }
            };

            string actual = JsonSerializer.Serialize(obj, options);
            string expected = @"{""Dictionary"":{""1"":{""Id"":1},""2"":{""Id"":2}}}";

            Assert.Equal(expected, actual);
        }

        public enum Enum
        {
            Foo,
            Bar
        }

        [Fact]
        public void ReadEnumDictionary()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Foo"":""foo"",""Bar"":""bar""}";
            Dictionary<Enum, string> actual = JsonSerializer.Deserialize<Dictionary<Enum, string>>(json, options);

            Dictionary<Enum, string> expected = new Dictionary<Enum, string>
            {
                [Enum.Foo] = "foo",
                [Enum.Bar] = "bar"
            };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteEnumDictionary()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Dictionary<Enum, string> dictionary = new Dictionary<Enum, string>
            {
                [Enum.Foo] = "foo",
                [Enum.Bar] = "bar"
            };

            string actual = JsonSerializer.Serialize(dictionary, options);
            const string expected = @"{""Foo"":""foo"",""Bar"":""bar""}";

            Assert.Equal(expected, actual);
        }
    }
}
