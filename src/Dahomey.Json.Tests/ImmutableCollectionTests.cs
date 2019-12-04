using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class ImmutableCollectionTests
    {
        [Fact]
        public void WriteImmutableArrayOfInt32()
        {
            ImmutableArray<int> value = ImmutableArray.CreateRange(new[] { 1, 2, 3 });
            const string expected = "[1,2,3]";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            string actual = JsonSerializer.Serialize(value, value.GetType(), options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadImmutableArrayOfInt32()
        {
            ImmutableArray<int> expected = ImmutableArray.CreateRange(new[] { 1, 2, 3 });
            const string json = "[1,2,3]";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            ImmutableArray<int> actual = JsonSerializer.Deserialize<ImmutableArray<int>>(json, options);

            Assert.Equal(expected[0], actual[0]);
            Assert.Equal(expected[1], actual[1]);
            Assert.Equal(expected[2], actual[2]);
        }

        [Fact]
        public void WriteImmutableListOfInt32()
        {
            ImmutableList<int> value = ImmutableList.CreateRange(new[] { 1, 2, 3 });
            const string expected = "[1,2,3]";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            string actual = JsonSerializer.Serialize(value, value.GetType(), options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadImmutableListOfInt32()
        {
            ImmutableList<int> expected = ImmutableList.CreateRange(new[] { 1, 2, 3 });
            const string json = "[1,2,3]";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            ImmutableList<int> actual = JsonSerializer.Deserialize<ImmutableList<int>>(json, options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteImmutableHashSetOfInt32()
        {
            ImmutableHashSet<int> value = ImmutableHashSet.CreateRange(new[] { 1, 2, 3 });
            const string expected = "[1,2,3]";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            string actual = JsonSerializer.Serialize(value, value.GetType(), options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadImmutableHashSetOfInt32()
        {
            ImmutableHashSet<int> expected = ImmutableHashSet.CreateRange(new[] { 1, 2, 3 });
            const string json = "[1,2,3]";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            ImmutableHashSet<int> actual = JsonSerializer.Deserialize<ImmutableHashSet<int>>(json, options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteImmutableSortedSetOfInt32()
        {
            ImmutableSortedSet<int> value = ImmutableSortedSet.CreateRange(new[] { 1, 2, 3 });
            const string expected = "[1,2,3]";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            string actual = JsonSerializer.Serialize(value, value.GetType(), options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadImmutableSortedSetOfInt32()
        {
            ImmutableSortedSet<int> expected = ImmutableSortedSet.CreateRange(new[] { 1, 2, 3 });
            const string json = "[1,2,3]";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            ImmutableSortedSet<int> actual = JsonSerializer.Deserialize<ImmutableSortedSet<int>>(json, options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteImmutableDictionaryOfInt32()
        {
            ImmutableDictionary<int, int> value = ImmutableDictionary.CreateRange(new Dictionary<int, int>
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 }
            });
            const string expected = @"{""1"":1,""2"":2,""3"":3}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            string actual = JsonSerializer.Serialize(value, value.GetType(), options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadImmutableDictionaryOfInt32()
        {
            ImmutableDictionary<int, int> expected = ImmutableDictionary.CreateRange(new Dictionary<int, int>
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 }
            });
            const string json = @"{""1"":1,""2"":2,""3"":3}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            ImmutableDictionary<int, int> actual = JsonSerializer.Deserialize<ImmutableDictionary<int, int>>(json, options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WriteImmutableSortedDictionaryOfInt32()
        {
            ImmutableSortedDictionary<int, int> value = ImmutableSortedDictionary.CreateRange(new SortedDictionary<int, int>
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 }
            });
            const string expected = @"{""1"":1,""2"":2,""3"":3}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            string actual = JsonSerializer.Serialize(value, value.GetType(), options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReadImmutableSortedDictionaryOfInt32()
        {
            ImmutableSortedDictionary<int, int> expected = ImmutableSortedDictionary.CreateRange(new SortedDictionary<int, int>
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 }
            });
            const string json = @"{""1"":1,""2"":2,""3"":3}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            ImmutableSortedDictionary<int, int> actual = JsonSerializer.Deserialize<ImmutableSortedDictionary<int, int>>(json, options);

            Assert.Equal(expected, actual);
        }
    }
}
