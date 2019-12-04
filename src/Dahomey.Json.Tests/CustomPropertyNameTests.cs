using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class CustomPropertyNameTests
    {
        public class WeatherForecastWithPropertyNameAttribute
        {
            public DateTimeOffset Date { get; set; }
            public int TemperatureCelsius { get; set; }
            public string Summary { get; set; }
            [JsonPropertyName("Wind")]
            public int WindSpeed { get; set; }
        }

        [Fact]
        public void TestRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Date"":""2019-08-01T00:00:00-07:00"",""TemperatureCelsius"":25,""Summary"":""Hot"",""Wind"":35}";

            var weatherForecast = JsonSerializer.Deserialize<WeatherForecastWithPropertyNameAttribute>(json, options);

            Assert.Equal(35, weatherForecast.WindSpeed);
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            options.SetupExtensions();

            var weatherForecast = new WeatherForecastWithPropertyNameAttribute
            {
                Date = DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"),
                TemperatureCelsius = 25,
                Summary = "Hot",
                WindSpeed = 35
            };

            const string expected = @"{""date"":""2019-08-01T00:00:00-07:00"",""temperatureCelsius"":25,""summary"":""Hot"",""Wind"":35}";
            string actual = JsonSerializer.Serialize(weatherForecast, options);

            Assert.Equal(expected, actual);
        }
    }
}
