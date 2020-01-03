using System;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class CamelCaseTests
    {
        public class WeatherForecast
        {
            public DateTimeOffset Date { get; set; }
            public int TemperatureCelsius { get; set; }
            public string Summary { get; set; }
        }

        [Fact]
        public void TestRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            options.SetupExtensions();

            const string json = @"{""date"":""2019-08-01T00:00:00-07:00"",""temperatureCelsius"":25,""summary"":""Hot""}";
            WeatherForecast weatherForecast = JsonSerializer.Deserialize<WeatherForecast>(json, options);

            Assert.NotNull(weatherForecast);
            Assert.Equal(DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"), weatherForecast.Date);
            Assert.Equal(25, weatherForecast.TemperatureCelsius);
            Assert.Equal("Hot", weatherForecast.Summary);
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            options.SetupExtensions();

            WeatherForecast weatherForecast = new WeatherForecast
            {
                Date = DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"),
                TemperatureCelsius = 25,
                Summary = "Hot"
            };

            const string expected = @"{""date"":""2019-08-01T00:00:00-07:00"",""temperatureCelsius"":25,""summary"":""Hot""}";
            string actual = JsonSerializer.Serialize(weatherForecast, options);

            Assert.Equal(expected, actual);
        }
    }
}
