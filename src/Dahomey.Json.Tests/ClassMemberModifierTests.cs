using Xunit;
using System.Reflection;
using Dahomey.Json.Attributes;
using System.Text.Json;

namespace Dahomey.Json.Tests
{
    public class ClassMemberModifierTests
    {
        public class ObjectWithPrivateProperty
        {
            public int Id { get; set; }

            [JsonProperty]
            private int PrivateProp1 { get; set; }

            private int PrivateProp2 { get; set; }

            public ObjectWithPrivateProperty()
            {
            }

            public ObjectWithPrivateProperty(int privateProp1, int privateProp2)
            {
                PrivateProp1 = privateProp1;
                PrivateProp2 = privateProp2;
            }

            public int GetProp1()
            {
                return PrivateProp1;
            }

            public int GetProp2()
            {
                return PrivateProp2;
            }
        }

        [Fact]
        public void TestWritePrivateProperty()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            ObjectWithPrivateProperty obj = new ObjectWithPrivateProperty(2, 3)
            {
                Id = 1,
            };

            const string json = @"{""Id"":1,""PrivateProp1"":2}";

            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void TestReadPrivateProperty()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Id"":1,""PrivateProp1"":2}";

            ObjectWithPrivateProperty obj = Helper.Read<ObjectWithPrivateProperty>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(1, obj.Id);
            Assert.Equal(2, obj.GetProp1());
            Assert.Equal(0, obj.GetProp2());
        }

        public class ObjectWithPrivateField
        {
            public int Id;

            [JsonProperty]
            private int PrivateProp1;

            private int PrivateProp2;

            public ObjectWithPrivateField()
            {
            }

            public ObjectWithPrivateField(int privateProp1, int privateProp2)
            {
                PrivateProp1 = privateProp1;
                PrivateProp2 = privateProp2;
            }

            public int GetProp1()
            {
                return PrivateProp1;
            }

            public int GetProp2()
            {
                return PrivateProp2;
            }
        }

        [Fact]
        public void TestWritePrivateField()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            ObjectWithPrivateField obj = new ObjectWithPrivateField(2, 3)
            {
                Id = 1,
            };

            const string json = @"{""Id"":1,""PrivateProp1"":2}";

            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void TestReadPrivateField()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Id"":1,""PrivateProp1"":2}";

            ObjectWithPrivateField obj = Helper.Read<ObjectWithPrivateField>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(1, obj.Id);
            Assert.Equal(2, obj.GetProp1());
            Assert.Equal(0, obj.GetProp2());
        }

        public class ObjectWithReadOnlyField
        {
            public readonly int Id;

            public ObjectWithReadOnlyField()
            {
            }

            public ObjectWithReadOnlyField(int id)
            {
                Id = id;
            }
        }

        [Fact]
        public void TestWriteReadOnlyField()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            ObjectWithReadOnlyField obj = new ObjectWithReadOnlyField(1);
            const string json = @"{""Id"":1}";
            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void TestReadReadOnlyField()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Id"":1}";
            ObjectWithReadOnlyField obj = Helper.Read<ObjectWithReadOnlyField>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(0, obj.Id);
        }

        public class ObjectWithConstField
        {
            [JsonProperty]
            public const int Id = 1;
        }

        [Fact]
        public void TestWriteConstField()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            ObjectWithConstField obj = new ObjectWithConstField();
            const string json = @"{""Id"":1}";
            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void TestReadConstField()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Id"":1}";
            ObjectWithConstField obj = Helper.Read<ObjectWithConstField>(json, options);

            Assert.NotNull(obj);
        }

        public class ObjectWithStaticField
        {
            [JsonProperty]
            public static int Id = 1;
        }

        [Fact]
        public void TestWriteStaticField()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            ObjectWithStaticField obj = new ObjectWithStaticField();
            const string json = @"{""Id"":1}";
            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void TestReadStaticField()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Id"":1}";
            ObjectWithStaticField obj = Helper.Read<ObjectWithStaticField>(json, options);

            Assert.NotNull(obj);
        }

        public class ObjectWithStaticProperty
        {
            [JsonProperty]
            public static int Id { get; set; } = 1;
        }

        [Fact]
        public void TestWriteStaticProperty()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            ObjectWithStaticProperty obj = new ObjectWithStaticProperty();
            const string json = @"{""Id"":1}";
            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void TestReadStaticProperty()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Id"":1}";
            ObjectWithStaticProperty obj = Helper.Read<ObjectWithStaticProperty>(json, options);

            Assert.NotNull(obj);
        }

        private class Tree
        {
            public const string Id = "Tree.class";
            public readonly string Name = "LemonTree";
            public static int WhatEver = 12;
        }

        [Fact]
        public void TestWriteByApi()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<Tree>(objectMapping =>
            {
                objectMapping.AutoMap();
                objectMapping.ClearMemberMappings();
                objectMapping.MapMember(
                    typeof(Tree)
                        .GetField(nameof(Tree.Id), BindingFlags.Public | BindingFlags.Static),
                    typeof(string));
                objectMapping.MapMember(tree => tree.Name);
                objectMapping.MapMember(tree => Tree.WhatEver);
            });

            Tree obj = new Tree();
            const string json = @"{""Id"":""Tree.class"",""Name"":""LemonTree"",""WhatEver"":12}";
            Helper.TestWrite(obj, json, options);
        }
    }
}
