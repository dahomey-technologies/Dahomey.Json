using Dahomey.Json.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests
{
    public partial class ReferenceHandlingTests
    {
        private static readonly JsonSerializerOptions s_deserializerOptionsPreserve = new JsonSerializerOptions().SetupExtensions().SetReferenceHandling(ReferenceHandling.Preserve);

        private class Employee
        {
            public string Name { get; set; }
            public Employee Manager { get; set; }
            public Employee Manager2 { get; set; }
            public List<Employee> Subordinates { get; set; }
            public List<Employee> Subordinates2 { get; set; }
            public Dictionary<string, Employee> Contacts { get; set; }
            public Dictionary<string, Employee> Contacts2 { get; set; }
            //Properties with default value to verify they get overwritten when deserializing into them.
            public List<string> SubordinatesString { get; set; } = new List<string> { "Bob" };
            public Dictionary<string, string> ContactsString { get; set; } = new Dictionary<string, string>() { { "Bob", "555-5555" } };
        }

        public class EmployeeWithContacts
        {
            public string Name { get; set; }
            public EmployeeWithContacts Manager { get; set; }
            public List<EmployeeWithContacts> Subordinates { get; set; }
            public Dictionary<string, EmployeeWithContacts> Contacts { get; set; }
        }

        [Fact]
        public static void ReadPreservingReference()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetReferenceHandling(ReferenceHandling.Preserve);

            const string json = @"{""$id"":""1"",""Name"":""Angela"",""Manager"":{""$id"":""2"",""Name"":""Bob"",""Subordinates"":{""$id"":""3"",""$values"":[{""$ref"":""1""}]}}}";
            var angela = JsonSerializer.Deserialize<Employee>(json, options);

            Assert.NotNull(angela);
            Assert.Equal("Angela", angela.Name);
            Assert.NotNull(angela.Manager);
            Assert.Equal("Bob", angela.Manager.Name);
            Assert.NotNull(angela.Manager.Subordinates);
            Assert.Single(angela.Manager.Subordinates);
            Assert.Same(angela, angela.Manager.Subordinates[0]);
        }

        [Fact]
        public static void ObjectWithoutMetadata()
        {
            string json = "{}";
            Employee employee = JsonSerializer.Deserialize<Employee>(json, s_deserializerOptionsPreserve);
            Assert.NotNull(employee);
        }

        [Fact] //Employee list as a property and then use reference to itself on nested Employee.
        public static void ObjectReferenceLoop()
        {
            string json =
            @"{
                ""$id"": ""1"",
                ""Name"": ""Angela"",
                ""Manager"": {
                    ""$ref"": ""1""
                }
            }";

            Employee angela = JsonSerializer.Deserialize<Employee>(json, s_deserializerOptionsPreserve);
            Assert.Same(angela, angela.Manager);
        }

        [Fact] // Employee whose subordinates is a preserved list. EmployeeListEmployee
        public static void ObjectReferenceLoopInList()
        {
            string json =
            @"{
                ""$id"": ""1"",
                ""Subordinates"": {
                    ""$id"": ""2"",
                    ""$values"": [
                        {
                            ""$ref"": ""1""
                        }
                    ]
                }
            }";

            Employee employee = JsonSerializer.Deserialize<Employee>(json, s_deserializerOptionsPreserve);
            Assert.Single(employee.Subordinates);
            Assert.Same(employee, employee.Subordinates[0]);
        }

        [Fact] // Employee whose subordinates is a preserved list. EmployeeListEmployee
        public static void ObjectReferenceLoopInDictionary()
        {
            string json =
            @"{
                ""$id"": ""1"",
                ""Contacts"":{
                    ""$id"": ""2"",
                    ""Angela"":{
                        ""$ref"": ""1""
                    }
                }
            }";

            EmployeeWithContacts employee = JsonSerializer.Deserialize<EmployeeWithContacts>(json, s_deserializerOptionsPreserve);
            Assert.Same(employee, employee.Contacts["Angela"]);
        }

        [Fact] //Employee list as a property and then use reference to itself on nested Employee.
        public static void ObjectWithArrayReferenceDeeper()
        {
            string json =
            @"{
                ""$id"": ""1"",
                ""Subordinates"": {
                    ""$id"": ""2"",
                    ""$values"": [
                        {
                            ""$id"": ""3"",
                            ""Name"": ""Angela"",
                            ""Subordinates"":{
                                ""$ref"": ""2""
                            }
                        }
                    ]
                }
            }";

            Employee employee = JsonSerializer.Deserialize<Employee>(json, s_deserializerOptionsPreserve);
            Assert.Same(employee.Subordinates, employee.Subordinates[0].Subordinates);
        }

        [Fact]
        public static void ObjectWithDictionaryReferenceDeeper()
        {
            string json =
            @"{
                ""$id"": ""1"",
                ""Contacts"": {
                    ""$id"": ""2"",
                    ""Angela"": {
                        ""$id"": ""3"",
                        ""Name"": ""Angela"",
                        ""Contacts"": {
                            ""$ref"": ""2""
                        }
                    }
                }
            }";

            EmployeeWithContacts employee = JsonSerializer.Deserialize<EmployeeWithContacts>(json, s_deserializerOptionsPreserve);
            Assert.Same(employee.Contacts, employee.Contacts["Angela"].Contacts);
        }

        private class ClassWithSubsequentListProperties
        {
            public List<int> MyList { get; set; }
            public List<int> MyListCopy { get; set; }
        }

        [Fact]
        public static void PreservedArrayIntoArrayProperty()
        {
            string json = @"
            {
                ""MyList"": {
                    ""$id"": ""1"",
                    ""$values"": [
                        10,
                        20,
                        30,
                        40
                    ]
                },
                ""MyListCopy"": { ""$ref"": ""1"" }
            }";

            ClassWithSubsequentListProperties instance = JsonSerializer.Deserialize<ClassWithSubsequentListProperties>(json, s_deserializerOptionsPreserve);
            Assert.Equal(4, instance.MyList.Count);
            Assert.Same(instance.MyList, instance.MyListCopy);
        }

        [Fact]
        public static void PreservedArrayIntoInitializedProperty()
        {
            string json = @"{
                ""$id"": ""1"",
                ""SubordinatesString"": {
                    ""$id"": ""2"",
                    ""$values"": [
                    ]
                },
                ""Manager"": {
                    ""SubordinatesString"":{
                        ""$ref"": ""2""
                    }
                }
            }";

            Employee employee = JsonSerializer.Deserialize<Employee>(json, s_deserializerOptionsPreserve);
            // presereved array.
            Assert.Empty(employee.SubordinatesString);
            // reference to preserved array.
            Assert.Empty(employee.Manager.SubordinatesString);
            Assert.Same(employee.Manager.SubordinatesString, employee.SubordinatesString);
        }

        [Fact] // Verify ReadStackFrame.DictionaryPropertyIsPreserved is being reset properly.
        public static void DictionaryPropertyOneAfterAnother()
        {
            string json = @"{
                ""$id"": ""1"",
                ""Contacts"": {
                    ""$id"": ""2""
                },
                ""Contacts2"": {
                    ""$ref"": ""2""
                }
            }";

            Employee employee = JsonSerializer.Deserialize<Employee>(json, s_deserializerOptionsPreserve);
            Assert.Same(employee.Contacts, employee.Contacts2);

            json = @"{
                ""$id"": ""1"",
                ""Contacts"": {
                    ""$id"": ""2""
                },
                ""Contacts2"": {
                    ""$id"": ""3""
                }
            }";

            employee = JsonSerializer.Deserialize<Employee>(json, s_deserializerOptionsPreserve);
            Assert.Empty(employee.Contacts);
            Assert.Empty(employee.Contacts2);
        }

        //[Fact]
        //public static async Task TestJsonPathDoesNotFailOnMultiThreads()
        //{
        //    const int ThreadCount = 8;
        //    const int ConcurrentTestsCount = 4;
        //    Task[] tasks = new Task[ThreadCount * ConcurrentTestsCount];

        //    for (int i = 0; i < tasks.Length; i++)
        //    {
        //        tasks[i++] = Task.Run(() => TestIdTask());
        //        tasks[i++] = Task.Run(() => TestRefTask());
        //        tasks[i++] = Task.Run(() => TestIdTask());
        //        tasks[i] = Task.Run(() => TestRefTask());
        //    }

        //    await Task.WhenAll(tasks);
        //}

        //private static void TestIdTask()
        //{
        //    JsonException ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Employee>(@"{""$id"":1}", s_deserializerOptionsPreserve));
        //    Assert.Equal("$.$id", ex.Path);
        //}

        //private static void TestRefTask()
        //{
        //    JsonException ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Employee>(@"{""$ref"":1}", s_deserializerOptionsPreserve));
        //    Assert.Equal("$.$ref", ex.Path);
        //}

        [Fact]
        public static void DictionaryWithoutMetadata()
        {
            string json = "{}";
            Dictionary<string, string> dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(json, s_deserializerOptionsPreserve);
            Assert.NotNull(dictionary);
        }

        [Fact] //Employee list as a property and then use reference to itself on nested Employee.
        public static void DictionaryReferenceLoop()
        {
            string json =
            @"{
                ""$id"": ""1"",
                ""Angela"": {
                    ""$id"": ""2"",
                    ""Name"": ""Angela"",
                    ""Contacts"": {
                        ""$ref"": ""1""
                    }
                }
            }";

            Dictionary<string, EmployeeWithContacts> dictionary = JsonSerializer.Deserialize<Dictionary<string, EmployeeWithContacts>>(json, s_deserializerOptionsPreserve);

            Assert.Same(dictionary, dictionary["Angela"].Contacts);
        }

        [Fact]
        public static void DictionaryReferenceLoopInList()
        {
            string json =
            @"{
                ""$id"": ""1"",
                ""Angela"": {
                    ""$id"": ""2"",
                    ""Name"": ""Angela"",
                    ""Subordinates"": {
                        ""$id"": ""3"",
                        ""$values"": [
                            {
                                ""$id"": ""4"",
                                ""Name"": ""Bob"",
                                ""Contacts"": {
                                    ""$ref"": ""1""
                                }
                            }
                        ]
                    }
                }
            }";

            Dictionary<string, EmployeeWithContacts> dictionary = JsonSerializer.Deserialize<Dictionary<string, EmployeeWithContacts>>(json, s_deserializerOptionsPreserve);
            Assert.Same(dictionary, dictionary["Angela"].Subordinates[0].Contacts);
        }

        [Fact]
        public static void DictionaryDuplicatedObject()
        {
            string json =
            @"{
              ""555"": { ""$id"": ""1"", ""Name"": ""Angela"" },
              ""556"": { ""Name"": ""Bob"" },
              ""557"": { ""$ref"": ""1"" }
            }";

            Dictionary<string, Employee> directory = JsonSerializer.Deserialize<Dictionary<string, Employee>>(json, s_deserializerOptionsPreserve);
            Assert.Same(directory["555"], directory["557"]);
        }

        [Fact]
        public static void DictionaryOfArrays()
        {
            string json =
            @"{
                ""$id"": ""1"",
                ""Array1"": {
                    ""$id"": ""2"",
                    ""$values"": []
                },
                ""Array2"": {
                    ""$ref"": ""2""
                }
            }";

            Dictionary<string, List<int>> dict = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(json, s_deserializerOptionsPreserve);
            Assert.Same(dict["Array1"], dict["Array2"]);
        }

        [Fact]
        public static void DictionaryOfDictionaries()
        {
            string json = @"{
                ""$id"": ""1"",
                ""Dictionary1"": {
                    ""$id"": ""2"",
                    ""value1"": 1,
                    ""value2"": 2,
                    ""value3"": 3
                },
                ""Dictionary2"": {
                    ""$ref"": ""2""
                }
            }";

            Dictionary<string, Dictionary<string, int>> root = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, int>>>(json, s_deserializerOptionsPreserve);
            Assert.Same(root["Dictionary1"], root["Dictionary2"]);
        }

        [Fact]
        public static void PreservedArrayIntoRootArray()
        {
            string json = @"
            {
                ""$id"": ""1"",
                ""$values"": [
                    10,
                    20,
                    30,
                    40
                ]
            }";

            List<int> myList = JsonSerializer.Deserialize<List<int>>(json, s_deserializerOptionsPreserve);
            Assert.Equal(4, myList.Count);
        }

        [Fact] // Preserved list that contains an employee whose subordinates is a reference to the root list.
        public static void ArrayNestedArray()
        {
            string json =
            @"{
                ""$id"": ""1"",
                ""$values"":[
                    {
                        ""$id"":""2"",
                        ""Name"": ""Angela"",
                        ""Subordinates"": {
                            ""$ref"": ""1""
                        }
                    }
                ]
            }";

            List<Employee> employees = JsonSerializer.Deserialize<List<Employee>>(json, s_deserializerOptionsPreserve);

            Assert.Same(employees, employees[0].Subordinates);
        }

        [Fact]
        public static void EmptyArray()
        {
            string json =
            @"{
              ""$id"": ""1"",
              ""Subordinates"": {
                ""$id"": ""2"",
                ""$values"": []
              },
              ""Name"": ""Angela""
            }";

            Employee angela = JsonSerializer.Deserialize<Employee>(json, s_deserializerOptionsPreserve);

            Assert.NotNull(angela);
            Assert.NotNull(angela.Subordinates);
            Assert.Empty(angela.Subordinates);
        }

        [Fact]
        public static void ArrayWithDuplicates() //Make sure the serializer can understand lists that were wrapped in braces.
        {
            string json =
            @"{
                ""$id"": ""1"",
                ""$values"":[
                    {
                        ""$id"": ""2"",
                        ""Name"": ""Angela""
                    },
                    {
                        ""$id"": ""3"",
                        ""Name"": ""Bob""
                    },
                    {
                        ""$ref"": ""2""
                    },
                    {
                        ""$ref"": ""3""
                    },
                    {
                        ""$id"": ""4""
                    },
                    {
                        ""$ref"": ""4""
                    }
                ]
            }";

            List<Employee> employees = JsonSerializer.Deserialize<List<Employee>>(json, s_deserializerOptionsPreserve);
            Assert.Equal(6, employees.Count);
            Assert.Same(employees[0], employees[2]);
            Assert.Same(employees[1], employees[3]);
            Assert.Same(employees[4], employees[5]);
        }

        [Fact]
        public static void ArrayNotPreservedWithDuplicates() //Make sure the serializer can understand lists that were wrapped in braces.
        {
            string json =
            @"[
                {
                    ""$id"": ""2"",
                    ""Name"": ""Angela""
                },
                {
                    ""$id"": ""3"",
                    ""Name"": ""Bob""
                },
                {
                    ""$ref"": ""2""
                },
                {
                    ""$ref"": ""3""
                },
                {
                    ""$id"": ""4""
                },
                {
                    ""$ref"": ""4""
                }
            ]";

            Employee[] employees = JsonSerializer.Deserialize<Employee[]>(json, s_deserializerOptionsPreserve);
            Assert.Equal(6, employees.Length);
            Assert.Same(employees[0], employees[2]);
            Assert.Same(employees[1], employees[3]);
            Assert.Same(employees[4], employees[5]);
        }

        [Fact]
        public static void ArrayWithNestedPreservedArray()
        {
            string json = @"{
                ""$id"": ""1"",
                ""$values"": [
                    {
                        ""$id"": ""2"",
                        ""$values"": [ 1, 2, 3 ]
                    }
                ]
            }";

            List<List<int>> root = JsonSerializer.Deserialize<List<List<int>>>(json, s_deserializerOptionsPreserve);
            Assert.Single(root);
            Assert.Equal(3, root[0].Count);
        }

        [Fact]
        public static void ArrayWithNestedPreservedArrayAndReference()
        {
            string json = @"{
                ""$id"": ""1"",
                ""$values"": [
                    {
                        ""$id"": ""2"",
                        ""$values"": [ 1, 2, 3 ]
                    },
                    { ""$ref"": ""2"" }
                ]
            }";

            List<List<int>> root = JsonSerializer.Deserialize<List<List<int>>>(json, s_deserializerOptionsPreserve);
            Assert.Equal(2, root.Count);
            Assert.Equal(3, root[0].Count);
            Assert.Same(root[0], root[1]);
        }

        private class ListWrapper
        {
            public List<List<int>> NestedList { get; set; } = new List<List<int>> { new List<int> { 1 } };
        }

        [Fact]
        public static void ArrayWithNestedPreservedArrayAndDefaultValues()
        {
            string json = @"{
                ""$id"": ""1"",
                ""NestedList"": {
                    ""$id"": ""2"",
                    ""$values"": [
                        {
                            ""$id"": ""3"",
                            ""$values"": [
                                1,
                                2,
                                3
                            ]
                        }
                    ]
                }
            }";

            ListWrapper root = JsonSerializer.Deserialize<ListWrapper>(json, s_deserializerOptionsPreserve);
            Assert.Single(root.NestedList);
            Assert.Equal(3, root.NestedList[0].Count);
        }

        [Fact]
        public static void ArrayWithMetadataWithinArray_UsingPreserve()
        {
            const string json =
            @"[
                {
                    ""$id"": ""1"",
                    ""$values"": []
                }
            ]";

            List<List<Employee>> root = JsonSerializer.Deserialize<List<List<Employee>>>(json, s_deserializerOptionsPreserve);
            Assert.Single(root);
            Assert.Empty(root[0]);
        }

        [Fact]
        public static void ObjectWithinArray_UsingDefault()
        {
            const string json =
            @"[
                {
                    ""$id"": ""1"",
                    ""$values"": []
                }
            ]";

            JsonException ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<List<List<Employee>>>(json));
            Assert.Equal("$[0]", ex.Path);
        }

        [Fact] //This only demonstrates that behavior with converters remain the same.
        public static void DeserializeWithListConverter()
        {
            string json =
            @"{
                ""$id"": ""1"",
                ""Subordinates"": {
                    ""$id"": ""2"",
                    ""$values"": [
                        {
                            ""$ref"": ""1""
                        }
                    ]
                },
                ""Name"": ""Angela"",
                ""Manager"": {
                    ""Subordinates"": {
                        ""$ref"": ""2""
                    }
                }
            }";

            var options = new JsonSerializerOptions
            {
                Converters = { new ListOfEmployeeConverter() }
            }.SetupExtensions().SetReferenceHandling(ReferenceHandling.Preserve);

            Employee angela = JsonSerializer.Deserialize<Employee>(json, options);
            Assert.Empty(angela.Subordinates);
            Assert.Empty(angela.Manager.Subordinates);
        }

        //NOTE: If you implement a converter, you are on your own when handling metadata properties and therefore references.Newtonsoft does the same.
        //However; is there a way to recall preserved references previously found in the payload and to store new ones found in the converter's payload? that would be a cool enhancement.
        private class ListOfEmployeeConverter : JsonConverter<List<Employee>>
        {
            public override List<Employee> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                int startObjectCount = 0;
                int endObjectCount = 0;

                while (true)
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.StartObject:
                            startObjectCount++; break;
                        case JsonTokenType.EndObject:
                            endObjectCount++; break;
                    }

                    if (startObjectCount == endObjectCount)
                    {
                        break;
                    }

                    reader.Read();
                }

                return new List<Employee>();
            }

            public override void Write(Utf8JsonWriter writer, List<Employee> value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }
}
