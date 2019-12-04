using System;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class ReadOnlyPropertyTests
    {
        public class WeatherForecastWithROProperty
        {
            public DateTimeOffset Date { get; set; }
            public int TemperatureCelsius { get; set; }
            public string Summary { get; set; }
            public int WindSpeedReadOnly { get; private set; } = 35;
        }

        [Fact]
        public void TestRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Date"":""2019-08-01T00:00:00-07:00"",""TemperatureCelsius"":25,""Summary"":""Hot""}";
            var weatherForecast = JsonSerializer.Deserialize<WeatherForecastWithROProperty>(json, options);

            Assert.NotNull(weatherForecast);
            Assert.Equal(DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"), weatherForecast.Date);
            Assert.Equal(25, weatherForecast.TemperatureCelsius);
            Assert.Equal("Hot", weatherForecast.Summary);
            Assert.Equal(35, weatherForecast.WindSpeedReadOnly);
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            var weatherForecast = new WeatherForecastWithROProperty
            {
                Date = DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"),
                TemperatureCelsius = 25,
                Summary = "Hot"
            };

            const string expected = @"{""Date"":""2019-08-01T00:00:00-07:00"",""TemperatureCelsius"":25,""Summary"":""Hot"",""WindSpeedReadOnly"":35}";
            string actual = JsonSerializer.Serialize(weatherForecast, options);

            Assert.Equal(expected, actual);
        }
    }
}
