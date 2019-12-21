using System.Runtime.Serialization;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class DataMemberTests
    {
        public class MyObject
        {
            [DataMember(Name = "bar")]
            public int Foo { get; set; }
        }

        [Fact]
        public void Read()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""bar"":12}";
            MyObject myObject = JsonSerializer.Deserialize<MyObject>(json, options);

            Assert.NotNull(myObject);
            Assert.Equal(12, myObject.Foo);
        }

        [Fact]
        public void Write()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""bar"":12}";
            MyObject myObject = new MyObject
            {
                Foo = 12
            };

            Helper.TestWrite(myObject, json, options);
        }
    }
}
