#if NET6_0_OR_GREATER

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class OrderTests
    {
        public class Account
        {
            public string EmailAddress { get; set; }

            // appear last
            [JsonPropertyOrder(1)]
            public bool Deleted { get; set; }

            [JsonPropertyOrder(2)]
            public DateTime DeletedDate { get; set; }

            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; }

            // appear first
            [JsonPropertyOrder(-2)]
            public string FullName { get; set; }
        }

        public class AccountByName
        {
            public string EmailAddress { get; set; }

            public bool Deleted { get; set; }

            public DateTime DeletedDate { get; set; }

            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; }

            public string FullName { get; set; }
        }

        [Fact]
        public void Order()
        {
            Account account = new()
            {
                FullName = "Aaron Account",
                EmailAddress = "aaron@example.com",
                Deleted = true,
                DeletedDate = new DateTime(2013, 1, 25),
                UpdatedDate = new DateTime(2013, 1, 25),
                CreatedDate = new DateTime(2010, 10, 1)
            };

            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string expected = @"{""FullName"":""Aaron Account"",""EmailAddress"":""aaron@example.com"",""CreatedDate"":""2010-10-01T00:00:00"",""UpdatedDate"":""2013-01-25T00:00:00"",""Deleted"":true,""DeletedDate"":""2013-01-25T00:00:00""}";
            string actual = JsonSerializer.Serialize(account, options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OrderByName()
        {
            AccountByName account = new()
            {
                FullName = "Aaron Account",
                EmailAddress = "aaron@example.com",
                Deleted = true,
                DeletedDate = new DateTime(2013, 1, 25),
                UpdatedDate = new DateTime(2013, 1, 25),
                CreatedDate = new DateTime(2010, 10, 1)
            };

            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetJsonPropertyOrderByName(true);

            const string expected = @"{""CreatedDate"":""2010-10-01T00:00:00"",""Deleted"":true,""DeletedDate"":""2013-01-25T00:00:00"",""EmailAddress"":""aaron@example.com"",""FullName"":""Aaron Account"",""UpdatedDate"":""2013-01-25T00:00:00""}";
            string actual = JsonSerializer.Serialize(account, options);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OrderByNameAndOrder()
        {
            Account account = new()
            {
                FullName = "Aaron Account",
                EmailAddress = "aaron@example.com",
                Deleted = true,
                DeletedDate = new DateTime(2013, 1, 25),
                UpdatedDate = new DateTime(2013, 1, 25),
                CreatedDate = new DateTime(2010, 10, 1)
            };

            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.SetJsonPropertyOrderByName(true);

            const string expected = @"{""FullName"":""Aaron Account"",""CreatedDate"":""2010-10-01T00:00:00"",""Deleted"":true,""DeletedDate"":""2013-01-25T00:00:00"",""EmailAddress"":""aaron@example.com"",""UpdatedDate"":""2013-01-25T00:00:00""}";
            string actual = JsonSerializer.Serialize(account, options);

            Assert.Equal(expected, actual);
        }
    }
}

#endif