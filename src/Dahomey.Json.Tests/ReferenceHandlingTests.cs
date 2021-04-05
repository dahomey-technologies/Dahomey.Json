using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Dahomey.Json.Tests
{
    public partial class ReferenceHandlingTests
    {
        [Fact]
        public static void ThrowByDefaultOnLoop()
        {
            Employee a = new Employee();
            a.Manager = a;

            JsonException ex = Assert.Throws<JsonException>(() => JsonSerializer.Serialize(a));
        }

        #region Root Object
        [Fact]
        public static void ObjectLoop()
        {
            Employee angela = new Employee();
            angela.Manager = angela;

            // Compare parity with Newtonsoft.Json
            string expected = Newtonsoft.Json.JsonConvert.SerializeObject(angela, s_newtonsoftSerializerSettingsPreserve);
            string actual = JsonSerializer.Serialize(angela, s_serializerOptionsPreserve);

            Assert.Equal(expected, actual);

            // Ensure round-trip
            Employee angelaCopy = JsonSerializer.Deserialize<Employee>(actual, s_serializerOptionsPreserve);
            Assert.Same(angelaCopy.Manager, angelaCopy);
        }

        [Fact]
        public static void ObjectArrayLoop()
        {
            Employee angela = new Employee();
            angela.Subordinates = new List<Employee> { angela };

            string expected = Newtonsoft.Json.JsonConvert.SerializeObject(angela, s_newtonsoftSerializerSettingsPreserve);
            string actual = JsonSerializer.Serialize(angela, s_serializerOptionsPreserve);

            Assert.Equal(expected, actual);

            Employee angelaCopy = JsonSerializer.Deserialize<Employee>(actual, s_serializerOptionsPreserve);
            Assert.Same(angelaCopy.Subordinates[0], angelaCopy);
        }

        [Fact]
        public static void ObjectDictionaryLoop()
        {
            Employee angela = new Employee();
            angela.Contacts = new Dictionary<string, Employee> { { "555-5555", angela } };

            string expected = Newtonsoft.Json.JsonConvert.SerializeObject(angela, s_newtonsoftSerializerSettingsPreserve);
            string actual = JsonSerializer.Serialize(angela, s_serializerOptionsPreserve);

            Assert.Equal(expected, actual);

            Employee angelaCopy = JsonSerializer.Deserialize<Employee>(actual, s_serializerOptionsPreserve);
            Assert.Same(angelaCopy.Contacts["555-5555"], angelaCopy);
        }

        [Fact]
        public static void ObjectPreserveDuplicateObjects()
        {
            Employee angela = new Employee
            {
                Manager = new Employee { Name = "Bob" }
            };
            angela.Manager2 = angela.Manager;

            string expected = Newtonsoft.Json.JsonConvert.SerializeObject(angela, s_newtonsoftSerializerSettingsPreserve);
            string actual = JsonSerializer.Serialize(angela, s_serializerOptionsPreserve);

            Assert.Equal(expected, actual);

            Employee angelaCopy = JsonSerializer.Deserialize<Employee>(actual, s_serializerOptionsPreserve);
            Assert.Same(angelaCopy.Manager, angelaCopy.Manager2);
        }

        [Fact]
        public static void ObjectPreserveDuplicateDictionaries()
        {
            Employee angela = new Employee
            {
                Contacts = new Dictionary<string, Employee> { { "444-4444", new Employee { Name = "Bob" } } }
            };
            angela.Contacts2 = angela.Contacts;

            string expected = Newtonsoft.Json.JsonConvert.SerializeObject(angela, s_newtonsoftSerializerSettingsPreserve);
            string actual = JsonSerializer.Serialize(angela, s_serializerOptionsPreserve);

            Assert.Equal(expected, actual);

            Employee angelaCopy = JsonSerializer.Deserialize<Employee>(actual, s_serializerOptionsPreserve);
            Assert.Same(angelaCopy.Contacts, angelaCopy.Contacts2);
        }

        [Fact]
        public static void ObjectPreserveDuplicateArrays()
        {
            Employee angela = new Employee
            {
                Subordinates = new List<Employee> { new Employee { Name = "Bob" } }
            };
            angela.Subordinates2 = angela.Subordinates;

            string expected = Newtonsoft.Json.JsonConvert.SerializeObject(angela, s_newtonsoftSerializerSettingsPreserve);
            string actual = JsonSerializer.Serialize(angela, s_serializerOptionsPreserve);

            Assert.Equal(expected, actual);

            Employee angelaCopy = JsonSerializer.Deserialize<Employee>(actual, s_serializerOptionsPreserve);
            Assert.Same(angelaCopy.Subordinates, angelaCopy.Subordinates2);
        }

        [Fact]
        public static void KeyValuePairTest()
        {
            var kvp = new KeyValuePair<string, string>("key", "value");
            string json = JsonSerializer.Serialize(kvp, s_deserializerOptionsPreserve);
            KeyValuePair<string, string> kvp2 = JsonSerializer.Deserialize<KeyValuePair<string, string>>(json, s_deserializerOptionsPreserve);

            Assert.Equal(kvp.Key, kvp2.Key);
            Assert.Equal(kvp.Value, kvp2.Value);
        }

        #endregion Root Object= Newtonsoft.Json.JsonConvert.SerializeObject

        #region Root Dictionary

        [Fact]
        public static void DictionaryObjectLoop()
        {
            Dictionary<string, Employee> root = new Dictionary<string, Employee>();
            root["Angela"] = new Employee() { Name = "Angela", Contacts = root };

            string expected = Newtonsoft.Json.JsonConvert.SerializeObject(root, s_newtonsoftSerializerSettingsPreserve);
            string actual = JsonSerializer.Serialize(root, s_serializerOptionsPreserve);

            Assert.Equal(expected, actual);

            Dictionary<string, Employee> rootCopy = JsonSerializer.Deserialize<Dictionary<string, Employee>>(actual, s_serializerOptionsPreserve);
            Assert.Same(rootCopy, rootCopy["Angela"].Contacts);
        }

        [Fact]
        public static void DictionaryPreserveDuplicateObjects()
        {
            Dictionary<string, Employee> root = new Dictionary<string, Employee>
            {
                ["Employee1"] = new Employee { Name = "Angela" }
            };
            root["Employee2"] = root["Employee1"];

            string expected = Newtonsoft.Json.JsonConvert.SerializeObject(root, s_newtonsoftSerializerSettingsPreserve);
            string actual = JsonSerializer.Serialize(root, s_serializerOptionsPreserve);

            Assert.Equal(expected, actual);

            Dictionary<string, Employee> rootCopy = JsonSerializer.Deserialize<Dictionary<string, Employee>>(actual, s_serializerOptionsPreserve);
            Assert.Same(rootCopy["Employee1"], rootCopy["Employee2"]);
        }

        [Fact]
        public static void DictionaryZeroLengthKey()
        {
            // Default

            Dictionary<string, int> rootValue = RoundTripDictionaryZeroLengthKey(new Dictionary<string, int>(), 10);
            Assert.Equal(10, rootValue[string.Empty]);

            Dictionary<string, Employee> rootObject = RoundTripDictionaryZeroLengthKey(new Dictionary<string, Employee>(), new Employee { Name = "Test" });
            Assert.Equal("Test", rootObject[string.Empty].Name);

            Dictionary<string, List<int>> rootArray = RoundTripDictionaryZeroLengthKey(new Dictionary<string, List<int>>(), new List<int>());
            Assert.Empty(rootArray[string.Empty]);

            // Preserve

            Dictionary<string, int> rootValue2 = RoundTripDictionaryZeroLengthKey(new Dictionary<string, int>(), 10, s_deserializerOptionsPreserve);
            Assert.Equal(10, rootValue2[string.Empty]);

            Dictionary<string, Employee> rootObject2 = RoundTripDictionaryZeroLengthKey(new Dictionary<string, Employee>(), new Employee { Name = "Test" }, s_deserializerOptionsPreserve);
            Assert.Equal("Test", rootObject2[string.Empty].Name);

            Dictionary<string, List<int>> rootArray2 = RoundTripDictionaryZeroLengthKey(new Dictionary<string, List<int>>(), new List<int>(), s_deserializerOptionsPreserve);
            Assert.Empty(rootArray2[string.Empty]);
        }

        private static Dictionary<string, TValue> RoundTripDictionaryZeroLengthKey<TValue>(Dictionary<string, TValue> dictionary, TValue value, JsonSerializerOptions opts = null)
        {
            dictionary[string.Empty] = value;
            string json = JsonSerializer.Serialize(dictionary, opts);
            Assert.Contains("\"\":", json);

            return JsonSerializer.Deserialize<Dictionary<string, TValue>>(json, opts);
        }

        [Fact]
        public static void UnicodeDictionaryKeys()
        {
            Dictionary<string, int> obj = new Dictionary<string, int> { { "A\u0467", 1 } };
            // Verify the name is escaped after serialize.
            string json = JsonSerializer.Serialize(obj, s_serializerOptionsPreserve);
            Assert.Equal(@"{""$id"":""1"",""A\u0467"":1}", json);

            // Round-trip
            Dictionary<string, int> objCopy = JsonSerializer.Deserialize<Dictionary<string, int>>(json, s_serializerOptionsPreserve);
            Assert.Equal(1, objCopy["A\u0467"]);

            // Verify with encoder.
            var optionsWithEncoder = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }.SetupExtensions().SetReferenceHandling(ReferenceHandling.Preserve);
            json = JsonSerializer.Serialize(obj, optionsWithEncoder);
            Assert.Equal("{\"$id\":\"1\",\"A\u0467\":1}", json);

            // Round-trip
            objCopy = JsonSerializer.Deserialize<Dictionary<string, int>>(json, optionsWithEncoder);
            Assert.Equal(1, objCopy["A\u0467"]);

            // We want to go over StackallocThreshold=256 to force a pooled allocation, so this property is 200 chars and 400 bytes.
            const int charsInProperty = 200;
            string longPropertyName = new string('\u0467', charsInProperty);
            obj = new Dictionary<string, int> { { $"{longPropertyName}", 1 } };
            Assert.Equal(1, obj[longPropertyName]);

            // Verify the name is escaped after serialize.
            json = JsonSerializer.Serialize(obj, s_serializerOptionsPreserve);

            // Duplicate the unicode character 'charsInProperty' times.
            string longPropertyNameEscaped = new StringBuilder().Insert(0, @"\u0467", charsInProperty).ToString();
            string expectedJson = $"{{\"$id\":\"1\",\"{longPropertyNameEscaped}\":1}}";
            Assert.Equal(expectedJson, json);

            // Round-trip
            objCopy = JsonSerializer.Deserialize<Dictionary<string, int>>(json, s_serializerOptionsPreserve);
            Assert.Equal(1, objCopy[longPropertyName]);

            // Verify the name is escaped after serialize.
            json = JsonSerializer.Serialize(obj, optionsWithEncoder);

            // Duplicate the unicode character 'charsInProperty' times.
            longPropertyNameEscaped = new StringBuilder().Insert(0, "\u0467", charsInProperty).ToString();
            expectedJson = $"{{\"$id\":\"1\",\"{longPropertyNameEscaped}\":1}}";
            Assert.Equal(expectedJson, json);

            // Round-trip
            objCopy = JsonSerializer.Deserialize<Dictionary<string, int>>(json, optionsWithEncoder);
            Assert.Equal(1, objCopy[longPropertyName]);
        }
        #endregion

        //[Fact]
        //public static async Task PreserveReferenceOfTypeObjectAsync()
        //{
        //    var root = new ClassWithObjectProperty();
        //    root.Child = new ClassWithObjectProperty();
        //    root.Sibling = root.Child;

        //    Assert.Same(root.Child, root.Sibling);

        //    var stream = new MemoryStream();
        //    await JsonSerializer.SerializeAsync(stream, root, s_serializerOptionsPreserve);

        //    string json = Encoding.UTF8.GetString(stream.ToArray());

        //    stream.Position = 0;

        //    ClassWithObjectProperty rootCopy = await JsonSerializer.DeserializeAsync<ClassWithObjectProperty>(stream, s_serializerOptionsPreserve);
        //    Assert.Same(rootCopy.Child, rootCopy.Sibling);
        //}

        [Fact]
        public static void DoNotPreserveReferenceWhenRefPropertyIsAbsent()
        {
            string json = @"{""Child"":{""$id"":""1""},""Sibling"":{""foo"":""1""}}";
            ClassWithObjectProperty root = JsonSerializer.Deserialize<ClassWithObjectProperty>(json);
            Assert.IsType<JsonElement>(root.Sibling);

            // $ref with any escaped character shall not be treated as metadata, hence Sibling must be JsonElement.
            json = @"{""Child"":{""$id"":""1""},""Sibling"":{""\\u0024ref"":""1""}}";
            root = JsonSerializer.Deserialize<ClassWithObjectProperty>(json);
            Assert.IsType<JsonElement>(root.Sibling);
        }

        //[Fact]
        //public static void VerifyValidationsOnPreservedReferenceOfTypeObject()
        //{
        //    const string baseJson = @"{""Child"":{""$id"":""1""},""Sibling"":";

        //    // A JSON object that contains a '$ref' metadata property must not contain any other properties.
        //    string testJson = baseJson + @"{""foo"":""value"",""$ref"":""1""}}";
        //    JsonException ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ClassWithObjectProperty>(testJson, s_serializerOptionsPreserve));
        //    Assert.Equal("$.Sibling", ex.Path);

        //    testJson = baseJson + @"{""$ref"":""1"",""bar"":""value""}}";
        //    ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ClassWithObjectProperty>(testJson, s_serializerOptionsPreserve));
        //    Assert.Equal("$.Sibling", ex.Path);

        //    // The '$id' and '$ref' metadata properties must be JSON strings.
        //    testJson = baseJson + @"{""$ref"":1}}";
        //    ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ClassWithObjectProperty>(testJson, s_serializerOptionsPreserve));
        //    Assert.Equal("$.Sibling", ex.Path);
        //}

        private class ClassWithObjectProperty
        {
            public ClassWithObjectProperty Child { get; set; }
            public object Sibling { get; set; }
        }

        private class ClassWithListOfObjectProperty
        {
            public ClassWithListOfObjectProperty Child { get; set; }
            public List<object> ListOfObjects { get; set; }
        }
    }
}
