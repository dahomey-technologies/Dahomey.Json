using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class AbstractClassTests
    {
        public interface Shape
        {
        }

        [JsonDiscriminator(nameof(Box))]
        public class Box : Shape
        {
            public float Width { get; set; }

            public float Height { get; set; }

            public override string ToString()
            {
                return $"Box: Width={Width}, Height={Height}";
            }
        }

        [JsonDiscriminator("Circle")]
        public class Circle : Shape
        {
            public float Radius { get; set; }

            public override string ToString()
            {
                return $"Circle: Radius={Radius}";
            }
        }

        [Fact]
        public void Test()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.ClearConventions();
            registry.RegisterConvention(new DefaultDiscriminatorConvention<string>(options, "Tag"));
            registry.RegisterType<Box>();
            registry.RegisterType<Circle>();

            Shape origin1 = new Box { Width = 10, Height = 20 };
            Shape origin2 = new Circle { Radius = 30 };

            string json1 = JsonSerializer.Serialize(origin1, options);
            string json2 = JsonSerializer.Serialize(origin2, options);

            Assert.Equal(@"{""Tag"":""Box"",""Width"":10,""Height"":20}", json1);
            Assert.Equal(@"{""Tag"":""Circle"",""Radius"":30}", json2);

            Shape restored1 = JsonSerializer.Deserialize<Shape>(json1, options);
            Shape restored2 = JsonSerializer.Deserialize<Shape>(json2, options);

            Assert.NotNull(restored1);
            Box restoredBox = Assert.IsType<Box>(restored1);
            Assert.Equal(10, restoredBox.Width);
            Assert.Equal(20, restoredBox.Height);

            Assert.NotNull(restored2);
            Circle restoredCircle = Assert.IsType<Circle>(restored2);
            Assert.Equal(30, restoredCircle.Radius);
        }

        public interface MyInterface
        {
            public int Id { get; set; }
        }

        public class MyImplementation : MyInterface
        {
            public int Id { get; set; }
        }

        [Fact]
        public void TestWriteInterface()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string expectedJson = @"{""Id"":12}";
            string actualJson = JsonSerializer.Serialize(new MyImplementation { Id = 12 }, typeof(MyInterface), options);

            Assert.Equal(expectedJson, actualJson);
        }

        public abstract class MyAbstractClass
        {
            public int Id { get; set; }
        }

        public class MyConcreteClass : MyAbstractClass
        {
        }

        [Fact]
        public void TestWriteAbstractClass()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string expectedJson = @"{""Id"":12}";
            string actualJson = JsonSerializer.Serialize(new MyConcreteClass { Id = 12 }, typeof(MyAbstractClass), options);

            Assert.Equal(expectedJson, actualJson);
        }

        public abstract class MyAbstractClass2
        {
            protected MyAbstractClass2(int id)
            {
                Id = id;
            }

            public int Id { get; set; }
        }

        public class MyConcreteClass2 : MyAbstractClass2
        {
            public MyConcreteClass2() : base(0)
            {
            }
        }

        [Fact]
        public void TestWriteAbstractClassWithNoDefaultCtor()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string expectedJson = @"{""Id"":12}";
            string actualJson = JsonSerializer.Serialize(new MyConcreteClass2 { Id = 12 }, typeof(MyAbstractClass2), options);

            Assert.Equal(expectedJson, actualJson);
        }
    }
}
