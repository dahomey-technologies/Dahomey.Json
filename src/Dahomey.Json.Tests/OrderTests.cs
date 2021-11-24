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

        private class Alphabet
        {
            public string A { get; set; } = "A";
            public string B { get; set; } = "B";
            public string C { get; set; } = "C";
            public string D { get; set; } = "D";
            public string E { get; set; } = "E";
            public string F { get; set; } = "F";
            public string G { get; set; } = "G";
            public string H { get; set; } = "H";
            public string I { get; set; } = "I";
            public string J { get; set; } = "J";
            public string K { get; set; } = "K";
            public string L { get; set; } = "L";
            public string M { get; set; } = "M";
            public string N { get; set; } = "N";
            public string O { get; set; } = "O";
            public string P { get; set; } = "P";
            public string Q { get; set; } = "Q";
            public string R { get; set; } = "R";
            public string S { get; set; } = "S";
            public string T { get; set; } = "T";
            public string U { get; set; } = "U";
            public string V { get; set; } = "V";
            public string W { get; set; } = "W";
            public string X { get; set; } = "X";
            public string Y { get; set; } = "Y";
            public string Z { get; set; } = "Z";
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
        public void OrderIsStable_Test()
        {
            // Arrange.
            var options = new JsonSerializerOptions().SetupExtensions();

            // Act.
            var alphabet = new Alphabet();
            var json = JsonSerializer.Serialize(alphabet, options);

            // Assert.
            const string expected = @"{""A"":""A"",""B"":""B"",""C"":""C"",""D"":""D"",""E"":""E"",""F"":""F"",""G"":""G"",""H"":""H"",""I"":""I"",""J"":""J"",""K"":""K"",""L"":""L"",""M"":""M"",""N"":""N"",""O"":""O"",""P"":""P"",""Q"":""Q"",""R"":""R"",""S"":""S"",""T"":""T"",""U"":""U"",""V"":""V"",""W"":""W"",""X"":""X"",""Y"":""Y"",""Z"":""Z""}";
            Assert.Equal(expected, json);
        }
    }
}

#endif