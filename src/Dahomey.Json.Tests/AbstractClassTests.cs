using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using System;
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
            registry.RegisterConvention(new AttributeBasedDiscriminatorConvention<string>(options, "Tag"));
            registry.RegisterType<Box>();
            registry.RegisterType<Circle>();

            Shape origin1 = new Box { Width = 10, Height = 20 };
            Shape origin2 = new Circle { Radius = 30 };

            string json1 = JsonSerializer.Serialize(origin1, options);
            string json2 = JsonSerializer.Serialize(origin2, options);

            Console.WriteLine(json1); // {"Tag":"Box","Width":10,"Height":20}
            Console.WriteLine(json2); // {"Tag":"Circle","Radius":30}

            var restored1 = JsonSerializer.Deserialize<Shape>(json1, options);
            var restored2 = JsonSerializer.Deserialize<Shape>(json2, options);

            Console.WriteLine(restored1); // Box: Width=10, Height=20
            Console.WriteLine(restored2); // Circle: Radius=30
        }
    }
}
