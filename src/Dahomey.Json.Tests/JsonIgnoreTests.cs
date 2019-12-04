using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class JsonIgnoreTests
    {
        public class WeatherForecastWithJsonIgnore
        {
            public WeatherForecastWithJsonIgnore()
            {
                Summary = "No summary";
            }

            public DateTimeOffset Date { get; set; }
            public int TemperatureCelsius { get; set; }

            [JsonIgnore]
            public string Summary { get; set; }
        }

        [Fact]
        public void TestRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Date"":""2019-08-01T00:00:00-07:00"",""TemperatureCelsius"":25}";
            var weatherForecast = JsonSerializer.Deserialize<WeatherForecastWithJsonIgnore>(json, options);

            Assert.NotNull(weatherForecast);
            Assert.Equal("No summary", weatherForecast.Summary);
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            var weatherForecast = new WeatherForecastWithJsonIgnore
            {
                Date = DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"),
                TemperatureCelsius = 25,
                Summary = "No summary"
            };

            const string expected = @"{""Date"":""2019-08-01T00:00:00-07:00"",""TemperatureCelsius"":25}";
            string actual = JsonSerializer.Serialize(weatherForecast, options);

            Assert.Equal(expected, actual);
        }
    }
}
