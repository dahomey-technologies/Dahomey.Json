using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0034
    {
        public class Response
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("data")]
            public IResultData Data { get; set; }
        }
        public interface IResultData { }

        [JsonDiscriminator("message")]
        public class MessageResult : IResultData
        {
            [JsonPropertyName("message_id")]
            public int MessageId { get; set; }

            [JsonPropertyName("message_content")]
            public int Content { get; set; }
        }

        [JsonDiscriminator("status")]
        public class StatusResult : IResultData
        {
            [JsonPropertyName("status_id")]
            public int StatusId { get; set; }
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.ClearConventions();
            registry.RegisterConvention(new DefaultDiscriminatorConvention<string>(options, "$type"));
            registry.RegisterType<MessageResult>();
            registry.RegisterType<StatusResult>();

            const string json1 = "{\"data\":{\"$type\":\"message\",\"message_id\":770,\"message_content\":1},\"id\":\"6nd28c1\",\"status\":\"ok\"}";
            Response response1 = JsonSerializer.Deserialize<Response>(json1, options);
            Assert.NotNull(response1);
            Assert.Equal("6nd28c1", response1.Id);
            Assert.Equal("ok", response1.Status);
            Assert.NotNull(response1.Data);
            MessageResult messageResult = Assert.IsType<MessageResult>(response1.Data);
            Assert.Equal(770, messageResult.MessageId);
            Assert.Equal(1, messageResult.Content);

            const string json2 = "{\"data\":{\"$type\":\"status\",\"status_id\":12},\"id\":\"6nd28c1\",\"status\":\"ok\"}";
            Response response2 = JsonSerializer.Deserialize<Response>(json2, options);
            Assert.NotNull(response2);
            Assert.Equal("6nd28c1", response2.Id);
            Assert.Equal("ok", response2.Status);
            Assert.NotNull(response2.Data);
            StatusResult statusResult = Assert.IsType<StatusResult>(response2.Data);
            Assert.Equal(12, statusResult.StatusId);
        }
    }
}
