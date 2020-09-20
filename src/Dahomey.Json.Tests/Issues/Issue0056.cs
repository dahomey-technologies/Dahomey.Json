using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0056
    {
        public class Car
        {
            public string Color { get; }
            public int? Age { get; }

            public Car(string color, string age)
            {
                Color = color;
                Age = int.Parse(age);
            }
        }

        [Fact]
        public void Test()
        {
            var options = new JsonSerializerOptions().SetupExtensions();
            options.GetObjectMappingRegistry().Register<Car>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .SetCreatorMapping(null)
            );
            var jsonString = JsonSerializer.Serialize(new Car("red", "4"), options);
        }
    }
}
