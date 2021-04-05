using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public partial class ReferenceHandlingTests
    {
        private static readonly JsonSerializerOptions s_serializerOptionsPreserve = new JsonSerializerOptions().SetupExtensions().SetReferenceHandling(ReferenceHandling.Preserve);
        private static readonly Newtonsoft.Json.JsonSerializerSettings s_newtonsoftSerializerSettingsPreserve = new Newtonsoft.Json.JsonSerializerSettings 
        { 
            PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All, 
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize
        };

        //[Fact]
        //public static void WriteIgnoringReferenceLoops()
        //{
        //    JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
        //    options.SetReferenceHandling(ReferenceHandling.Ignore);

        //    var bob = new Employee { Name = "Bob", SubordinatesString = null, ContactsString = null };
        //    var angela = new Employee { Name = "Angela", SubordinatesString = null, ContactsString = null };

        //    angela.Manager = bob;
        //    bob.Subordinates = new List<Employee> { angela };

        //    const string json = @"{""Name"":""Angela"",""Manager"":{""Name"":""Bob"",""Subordinates"":[]}}";
        //    Helper.TestWrite(angela, json, options);
        //}

        //[Fact]
        //public static void WritePreservingReference()
        //{
        //    JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
        //    options.SetReferenceHandling(ReferenceHandling.Preserve);

        //    var bob = new Employee { Name = "Bob", SubordinatesString = null, ContactsString = null };
        //    var angela = new Employee { Name = "Angela", SubordinatesString = null, ContactsString = null };

        //    angela.Manager = bob;
        //    bob.Subordinates = new List<Employee> { angela };

        //    const string json = @"{""$id"":""1"",""Name"":""Angela"",""Manager"":{""$id"":""2"",""Name"":""Bob"",""Subordinates"":{""$id"":""3"",""$values"":[{""$ref"":""1""}]}}}";
        //    Helper.TestWrite(angela, json, options);
        //}

        private class EmployeeExtensionData : Employee
        {
            [System.Text.Json.Serialization.JsonExtensionData]
            [Newtonsoft.Json.JsonExtensionData]
            public IDictionary<string, object> ExtensionData { get; set; }
        }

        [Fact]
        public static void ExtensionDataDictionaryHandlesPreserveReferences()
        {
            Employee bob = new Employee { Name = "Bob" };

            EmployeeExtensionData angela = new EmployeeExtensionData
            {
                Name = "Angela",

                Manager = bob
            };
            bob.Subordinates = new List<Employee> { angela };

            var extensionData = new Dictionary<string, object>
            {
                ["extString"] = "string value",
                ["extNumber"] = 100,
                ["extObject"] = bob,
                ["extArray"] = bob.Subordinates
            };

            angela.ExtensionData = extensionData;

            string expected = Newtonsoft.Json.JsonConvert.SerializeObject(angela, s_newtonsoftSerializerSettingsPreserve);
            string actual = JsonSerializer.Serialize(angela, s_serializerOptionsPreserve);

            Assert.Equal(expected, actual);
        }

        #region struct tests
        private struct EmployeeStruct
        {
            public string Name { get; set; }
            public JobStruct Job { get; set; }
            public ImmutableArray<RoleStruct> Roles { get; set; }
        }

        private struct JobStruct
        {
            public string Title { get; set; }
        }

        private struct RoleStruct
        {
            public string Description { get; set; }
        }

        [Fact]
        public static void ValueTypesShouldNotContainId()
        {
            //Struct as root.
            EmployeeStruct employee = new EmployeeStruct
            {
                Name = "Angela",
                //Struct as property.
                Job = new JobStruct
                {
                    Title = "Software Engineer"
                },
                //ImmutableArray<T> as property.
                Roles =
                    ImmutableArray.Create(
                        new RoleStruct
                        {
                            Description = "Contributor"
                        },
                        new RoleStruct
                        {
                            Description = "Infrastructure"
                        })
            };

            //ImmutableArray<T> as root.
            ImmutableArray<EmployeeStruct> array =
            //Struct as array element (same as struct being root).
            ImmutableArray.Create(employee);

            // Regardless of using preserve, do not emit $id to value types; that is why we compare against default.
            string actual = JsonSerializer.Serialize(array, s_serializerOptionsPreserve);
            string expected = JsonSerializer.Serialize(array);

            Assert.Equal(expected, actual);
        }

        #endregion struct tests

        private class ClassWithListAndImmutableArray
        {
            public List<int> PreservableList { get; set; }
            public ImmutableArray<int> NonProservableArray { get; set; }
        }

        [Fact]
        public static void WriteWrappingBraceResetsCorrectly()
        {
            List<int> list = new List<int> { 10, 20, 30 };
            ImmutableArray<int> immutableArr = list.ToImmutableArray();

            var root = new ClassWithListAndImmutableArray
            {
                PreservableList = list,
                // Do not write any curly braces for ImmutableArray since is a value type.
                NonProservableArray = immutableArr
            };
            JsonSerializer.Serialize(root, s_serializerOptionsPreserve);

            ImmutableArray<List<int>> immutablArraytOfLists = new List<List<int>> { list }.ToImmutableArray();
            JsonSerializer.Serialize(immutablArraytOfLists, s_serializerOptionsPreserve);

            List<ImmutableArray<int>> listOfImmutableArrays = new List<ImmutableArray<int>> { immutableArr };
            JsonSerializer.Serialize(listOfImmutableArrays, s_serializerOptionsPreserve);

            List<object> mixedListOfLists = new List<object> { list, immutableArr, list, immutableArr };
            JsonSerializer.Serialize(mixedListOfLists, s_serializerOptionsPreserve);
        }
    }
}
