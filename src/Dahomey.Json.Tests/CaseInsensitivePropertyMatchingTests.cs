using System;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class CaseInsensitivePropertyMatchingTests
    {
        public class WeatherForecastWithDefault
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
                PropertyNameCaseInsensitive = true
            };
            options.SetupExtensions();

            const string json = @"{""date"":""2019-08-01T00:00:00-07:00"",""TEMPERATURECELSIUS"":25,""Summary"":""Hot""}";
            WeatherForecastWithDefault weatherForecast = JsonSerializer.Deserialize<WeatherForecastWithDefault>(json, options);

            Assert.NotNull(weatherForecast);
            Assert.Equal(DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"), weatherForecast.Date);
            Assert.Equal(25, weatherForecast.TemperatureCelsius);
            Assert.Equal("Hot", weatherForecast.Summary);
        }
    }
}
