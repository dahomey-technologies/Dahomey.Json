using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class CamelCaseDictionaryKeysTests
    {
        public class WeatherForecast
        {
            public DateTimeOffset Date { get; set; }
            public int TemperatureCelsius { get; set; }
            public string Summary { get; set; }
            public Dictionary<string, int> TemperatureRanges { get; set; }
        }

        [Fact]
        public void TestWrite()
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            };
            options.SetupExtensions();

            WeatherForecast weatherForecast = new WeatherForecast
            {
                Date = DateTimeOffset.Parse("2019-08-01T00:00:00-07:00"),
                TemperatureCelsius = 25,
                Summary = "Hot",
                TemperatureRanges = new Dictionary<string, int>
                {
                    ["ColdMinTemp" ] = 20,
                    ["hotMinTemp"] = 40,
                }
            };

            const string expected = @"{""Date"":""2019-08-01T00:00:00-07:00"",""TemperatureCelsius"":25,""Summary"":""Hot"",""TemperatureRanges"":{""coldMinTemp"":20,""hotMinTemp"":40}}";
            string actual = JsonSerializer.Serialize(weatherForecast, options);

            Assert.Equal(expected, actual);
        }
    }
}
