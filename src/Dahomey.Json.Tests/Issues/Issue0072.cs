using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0072
    {
#if NET5_0_OR_GREATER
        public class Test
        {
            public string Prop1 { get; set; } = null;
            public bool? Prop2 { get; set; } = null;
            public bool? Prop3 { get; set; } = false;
        }

        [Fact]
        public void TestWrite()
        {
            Test obj = new Test();
            const string json = @"{""Prop3"":false}";
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            Helper.TestWrite(obj, json, options);
        }
#endif
    }
}
