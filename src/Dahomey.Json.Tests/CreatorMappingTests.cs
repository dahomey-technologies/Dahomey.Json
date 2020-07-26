using Dahomey.Json.Attributes;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class CreatorMappingTests
    {
        private class ObjectWithConstructor
        {
            private int id;
            private string name;

            public int Id => id;
            public string Name => name;
            public int Age { get; set; }

            public ObjectWithConstructor(int id, string name)
            {
                this.id = id;
                this.name = name;
            }
        }

        [Theory]
        [InlineData(@"{""Id"":12,""Name"":""foo""}", 12, "foo", 0)]
        [InlineData(@"{""Id"":12}", 12, null, 0)]
        [InlineData(@"{""Id"":12,""Name"":""foo"",""Age"":13}", 12, "foo", 13)]
        public void ConstructorByApi(string json, int expectedId, string expectedName, int expectedAge)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<ObjectWithConstructor>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .MapCreator(o => new ObjectWithConstructor(o.Id, o.Name))
            );

            ObjectWithConstructor obj = Helper.Read<ObjectWithConstructor>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(expectedId, obj.Id);
            Assert.Equal(expectedName, obj.Name);
            Assert.Equal(expectedAge, obj.Age);
        }

        private class ObjectWithConstructor2
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }

#if NET5_0
            [JsonConstructor]
#else
            [JsonConstructorEx]
#endif
            public ObjectWithConstructor2(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        [Theory]
        [InlineData(@"{""Id"":12,""Name"":""foo""}", 12, "foo", 0)]
        [InlineData(@"{""Id"":12}", 12, null, 0)]
        [InlineData(@"{""Id"":12,""Name"":""foo"",""Age"":13}", 12, "foo", 13)]
        public void ConstructorByAttribute(string json, int expectedId, string expectedName, int expectedAge)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            ObjectWithConstructor2 obj = Helper.Read<ObjectWithConstructor2>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(expectedId, obj.Id);
            Assert.Equal(expectedName, obj.Name);
            Assert.Equal(expectedAge, obj.Age);
        }

        private class ObjectWithConstructor3
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }

            [JsonConstructorEx(nameof(Id), nameof(Name))]
            public ObjectWithConstructor3(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        [Theory]
        [InlineData(@"{""Id"":12,""Name"":""foo""}", 12, "foo", 0)]
        [InlineData(@"{""Id"":12}", 12, null, 0)]
        [InlineData(@"{""Id"":12,""Name"":""foo"",""Age"":13}", 12, "foo", 13)]
        public void ConstructorByAttributeWithMemberNames(string json, int expectedId, string expectedName, int expectedAge)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            ObjectWithConstructor3 obj = Helper.Read<ObjectWithConstructor3>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(expectedId, obj.Id);
            Assert.Equal(expectedName, obj.Name);
            Assert.Equal(expectedAge, obj.Age);
        }

        private class Factory
        {
            public ObjectWithConstructor NewObjectWithConstructor(int id, string name)
            {
                return new ObjectWithConstructor(id, name);
            }
        }

        [Theory]
        [InlineData(@"{""Id"":12,""Name"":""foo""}", 12, "foo", 0)]
        [InlineData(@"{""Id"":12}", 12, null, 0)]
        [InlineData(@"{""Id"":12,""Name"":""foo"",""Age"":13}", 12, "foo", 13)]
        public void FactoryByApi(string json, int expectedId, string expectedName, int expectedAge)
        {
            Factory factory = new Factory();
            Func<int, string, ObjectWithConstructor> creatorFunc = (int id, string name) => factory.NewObjectWithConstructor(id, name);

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<ObjectWithConstructor>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .MapCreator(creatorFunc)
            );

            ObjectWithConstructor obj = Helper.Read<ObjectWithConstructor>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(expectedId, obj.Id);
            Assert.Equal(expectedName, obj.Name);
            Assert.Equal(expectedAge, obj.Age);
        }

        [Theory]
        [InlineData(@"{""Id"":12,""Name"":""foo""}", 12, "foo", 0)]
        [InlineData(@"{""Id"":12}", 12, null, 0)]
        [InlineData(@"{""Id"":12,""Name"":""foo"",""Age"":13}", 12, "foo", 13)]
        public void FactoryMethodByApi(string json, int expectedId, string expectedName, int expectedAge)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<ObjectWithConstructor>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .MapCreator(p => NewObjectWithConstructor(p.Id, p.Name))
            );

            ObjectWithConstructor obj = Helper.Read<ObjectWithConstructor>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(expectedId, obj.Id);
            Assert.Equal(expectedName, obj.Name);
            Assert.Equal(expectedAge, obj.Age);
        }

        private static ObjectWithConstructor NewObjectWithConstructor(int id, string name)
        {
            return new ObjectWithConstructor(id, name);
        }

        private interface IFoo
        {
            int Id { get; set; }
        }

        private class Foo : IFoo
        {
            public int Id { get; set; }
        }

        private class ObjectWithInterface
        {
            public IFoo Foo { get; set; }
        }

        [Fact]
        public void Interface()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<IFoo>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .MapCreator(o => new Foo())
            );

            const string json = @"{""Foo"":{""Id"":12}}";
            ObjectWithInterface obj = Helper.Read<ObjectWithInterface>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Foo);
            Assert.IsType<Foo>(obj.Foo);
            Assert.Equal(12, obj.Foo.Id);

            Helper.TestWrite(obj, json, options);
        }

        public abstract class AbstractBar
        {
            public int Id { get; set; }
        }

        public class Bar : AbstractBar
        {
        }

        public class ObjectWithAbstractClass
        {
            public AbstractBar Bar { get; set; }
        }

        [Fact]
        public void AbstractClass()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<AbstractBar>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .MapCreator(o => new Bar())
            );

            const string json = @"{""Bar"":{""Id"":12}}";
            ObjectWithAbstractClass obj = Helper.Read<ObjectWithAbstractClass>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Bar);
            Assert.IsType<Bar>(obj.Bar);
            Assert.Equal(12, obj.Bar.Id);

            Helper.TestWrite(obj, json, options);
        }
    }
}
