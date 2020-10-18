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

#if NET5_0

        public class MyClass
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
            public int Always { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
            public int Never { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public int WhenWritingDefault { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public object WhenWritingNull { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public int? WhenWritingNullableNull { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public int? WhenWritingNullableValue { get; set; } = 123;
        }

        [Fact]
        public void TestWritingCondition()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            MyClass myClass = new MyClass
            {
                Always = 12,
                Never = 0,
                WhenWritingDefault = 0,
                WhenWritingNull = null,
            };

            const string expected = @"{""Never"":0,""WhenWritingNullableValue"":123}";
            string actual = JsonSerializer.Serialize(myClass, options);

            Assert.Equal(expected, actual);
        }

#endif
    }
}
