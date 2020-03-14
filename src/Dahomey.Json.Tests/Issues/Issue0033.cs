using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0033
    {
        public class WeatherForecastWithExtensionDataAndIDictionary
        {
            public DateTimeOffset Date { get; set; }
            public int TemperatureCelsius { get; set; }
            public string Summary { get; set; }
            [JsonExtensionData]
            public IDictionary<string, object> ExtensionData { get; set; }
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Date"":""2019-08-01T00:00:00-07:00"",""TemperatureCelsius"":25,""Summary"":""Hot"",""DatesAvailable"":[""2019-08-01T00:00:00-07:00"",""2019-08-02T00:00:00-07:00""],""SummaryWords"":[""Cool"",""Windy"",""Humid""]}";
            var weatherForecast = new WeatherForecastWithExtensionDataAndIDictionary()
            {
                Summary = "Hot",
                Date = DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"),
                TemperatureCelsius = 25,
                ExtensionData = new Dictionary<string, object>()
                {
                    {"DatesAvailable", new List<string> { "2019-08-01T00:00:00-07:00", "2019-08-02T00:00:00-07:00" } },
                    {"SummaryWords", new List<string> { "Cool", "Windy", "Humid" }}
                }
            };
            var serialized = JsonSerializer.Serialize(weatherForecast, options);

            Assert.Equal(json, serialized);

        }

        [Fact]
        public void TestRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Date"":""2019-08-01T00:00:00-07:00"",""TemperatureCelsius"":25,""Summary"":""Hot"",""DatesAvailable"":[""2019-08-01T00:00:00-07:00"",""2019-08-02T00:00:00-07:00""],""SummaryWords"":[""Cool"",""Windy"",""Humid""]}";
            var weatherForecast = JsonSerializer.Deserialize<WeatherForecastWithExtensionDataAndIDictionary>(json, options);

            Assert.NotNull(weatherForecast);
            Assert.Equal(DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"), weatherForecast.Date);
            Assert.Equal(25, weatherForecast.TemperatureCelsius);
            Assert.Equal("Hot", weatherForecast.Summary);
            Assert.NotNull(weatherForecast.ExtensionData);
            Assert.Equal(2, weatherForecast.ExtensionData.Count);
            object value1 = Assert.Contains("DatesAvailable", (IDictionary<string, object>)weatherForecast.ExtensionData);
            Assert.IsType<JsonElement>(value1);
            object value2 = Assert.Contains("SummaryWords", (IDictionary<string, object>)weatherForecast.ExtensionData);
            Assert.IsType<JsonElement>(value2);
        }

        public class WeatherForecastWithExtensionDataAndIDictionary2
        {
            public DateTimeOffset Date { get; set; }
            public int TemperatureCelsius { get; set; }
            public string Summary { get; set; }
            [JsonExtensionData]
            public IDictionary<string, JsonElement> ExtensionData { get; set; }
        }

        [Fact]
        public void TestReadJsonElement()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Date"":""2019-08-01T00:00:00-07:00"",""TemperatureCelsius"":25,""Summary"":""Hot"",""DatesAvailable"":[""2019-08-01T00:00:00-07:00"",""2019-08-02T00:00:00-07:00""],""SummaryWords"":[""Cool"",""Windy"",""Humid""]}";
            var weatherForecast = JsonSerializer.Deserialize<WeatherForecastWithExtensionDataAndIDictionary2>(json, options);

            Assert.NotNull(weatherForecast);
            Assert.Equal(DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"), weatherForecast.Date);
            Assert.Equal(25, weatherForecast.TemperatureCelsius);
            Assert.Equal("Hot", weatherForecast.Summary);
            Assert.NotNull(weatherForecast.ExtensionData);
            Assert.Equal(2, weatherForecast.ExtensionData.Count);
            JsonElement value1 = Assert.Contains("DatesAvailable", (IDictionary<string, JsonElement>)weatherForecast.ExtensionData);
            JsonElement value2 = Assert.Contains("SummaryWords", (IDictionary<string, JsonElement>)weatherForecast.ExtensionData);
        }
    }
}

