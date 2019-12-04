using System;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class IgnoreNullValuesTests
    {
        public class WeatherForecastWithDefault
        {
            public WeatherForecastWithDefault()
            {
                Summary = "No summary";
            }
            public DateTimeOffset Date { get; set; }
            public int TemperatureCelsius { get; set; }
            public string Summary { get; set; }
        }

        [Fact]
        public void TestRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
            options.SetupExtensions();

            const string json = @"{""Date"":""2019-08-01T00:00:00-07:00"",""TemperatureCelsius"":25,""Summary"":null}";
            WeatherForecastWithDefault weatherForecast = JsonSerializer.Deserialize<WeatherForecastWithDefault>(json, options);

            Assert.NotNull(weatherForecast);
            Assert.Equal(DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"), weatherForecast.Date);
            Assert.Equal(25, weatherForecast.TemperatureCelsius);
            Assert.Equal("No summary", weatherForecast.Summary);
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
            options.SetupExtensions();

            WeatherForecastWithDefault weatherForecast = new WeatherForecastWithDefault
            {
                Date = DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"),
                TemperatureCelsius = 25,
                Summary = null
            };

            const string expected = @"{""Date"":""2019-08-01T00:00:00-07:00"",""TemperatureCelsius"":25}";
            string actual = JsonSerializer.Serialize(weatherForecast, options);

            Assert.Equal(expected, actual);
        }
    }
}
