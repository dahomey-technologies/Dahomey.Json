using Dahomey.Json.Attributes;
using System;
using System.Collections.Generic;
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

        public class SimpleTestClass
        {
            public int MyInt32 { get; set; }
        }

        public class SimpleTestClassUsingJsonDeserialize
        {
            [JsonDeserialize]
            public SimpleTestClass MyClass { get; } = new SimpleTestClass();
        }

        [Fact]
        public void TestReadUsingJsonDeserialize()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""MyClass"":{""MyInt32"":12}}";
            var obj = JsonSerializer.Deserialize<SimpleTestClassUsingJsonDeserialize>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.MyClass);
            Assert.Equal(12, obj.MyClass.MyInt32);
        }

        public class SimpleTestClassWithReadOnlyClass
        {
            public SimpleTestClass MyClass { get; } = new SimpleTestClass();
        }

        [Fact]
        public void TestReadUsingReadOnlyPropertyHandlingRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.SetReadOnlyPropertyHandling(ReadOnlyPropertyHandling.Read);

            const string json = @"{""MyClass"":{""MyInt32"":12}}";
            var obj = JsonSerializer.Deserialize<SimpleTestClassWithReadOnlyClass>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.MyClass);
            Assert.Equal(12, obj.MyClass.MyInt32);
        }

        [Fact]
        public void TestReadUsingReadOnlyPropertyHandlingIgnore()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.SetReadOnlyPropertyHandling(ReadOnlyPropertyHandling.Ignore);

            const string json = @"{""MyClass"":{""MyInt32"":12}}";
            var obj = JsonSerializer.Deserialize<SimpleTestClassWithReadOnlyClass>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.MyClass);
            Assert.Equal(default, obj.MyClass.MyInt32);
        }

        public class SimpleTestClassWithCollectionUsingJsonDeserialize
        {
            [JsonDeserialize]
            public IList<SimpleTestClass> MyClass { get; } = new List<SimpleTestClass>();
        }

        [Fact]
        public void TestReadWithCollectionUsingJsonDeserialize()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""MyClass"":[{""MyInt32"":12}]}";
            var obj = JsonSerializer.Deserialize<SimpleTestClassWithCollectionUsingJsonDeserialize>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.MyClass);
            Assert.Equal(1, obj.MyClass.Count);
            Assert.Equal(12, obj.MyClass[0].MyInt32);
        }

        public class SimpleTestClassWithReadOnlyCollectionProperty
        {
            public IList<SimpleTestClass> MyClass { get; } = new List<SimpleTestClass>();
        }

        [Fact]
        public void TestReadWithCollectionUsingReadOnlyPropertyHandlingRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.SetReadOnlyPropertyHandling(ReadOnlyPropertyHandling.Read);

            const string json = @"{""MyClass"":[{""MyInt32"":12}]}";
            var obj = JsonSerializer.Deserialize<SimpleTestClassWithReadOnlyCollectionProperty>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.MyClass);
            Assert.Equal(1, obj.MyClass.Count);
            Assert.Equal(12, obj.MyClass[0].MyInt32);
        }

        [Fact]
        public void TestReadWithCollectionUsingReadOnlyPropertyHandlingIgnore()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.SetReadOnlyPropertyHandling(ReadOnlyPropertyHandling.Ignore);

            const string json = @"{""MyClass"":[{""MyInt32"":12}]}";
            var obj = JsonSerializer.Deserialize<SimpleTestClassWithReadOnlyCollectionProperty>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.MyClass);
            Assert.Equal(0, obj.MyClass.Count);
        }

        public class SimpleTestClassWithDictionaryUsingJsonDeserialize
        {
            [JsonDeserialize]
            public IDictionary<string, SimpleTestClass> MyClass { get; } = new Dictionary<string, SimpleTestClass>();
        }

        [Fact]
        public void TestReadWithDictionaryUsingJsonDeserialize()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""MyClass"":{""MyKey"":{""MyInt32"":12}}}";
            var obj = JsonSerializer.Deserialize<SimpleTestClassWithDictionaryUsingJsonDeserialize>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.MyClass);
            Assert.Equal(1, obj.MyClass.Count);
            Assert.True(obj.MyClass.ContainsKey("MyKey"));
            Assert.Equal(12, obj.MyClass["MyKey"].MyInt32);
        }
        public class SimpleTestClassWithReadOnlyDictionaryProperty
        {
            public IDictionary<string, SimpleTestClass> MyClass { get; } = new Dictionary<string, SimpleTestClass>();
        }

        [Fact]
        public void TestReadWithDictionaryUsingReadOnlyPropertyHandlingRead()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.SetReadOnlyPropertyHandling(ReadOnlyPropertyHandling.Read);

            const string json = @"{""MyClass"":{""MyKey"":{""MyInt32"":12}}}";
            var obj = JsonSerializer.Deserialize<SimpleTestClassWithReadOnlyDictionaryProperty>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.MyClass);
            Assert.Equal(1, obj.MyClass.Count);
            Assert.True(obj.MyClass.ContainsKey("MyKey"));
            Assert.Equal(12, obj.MyClass["MyKey"].MyInt32);
        }

        [Fact]
        public void TestReadWithDictionaryUsingReadOnlyPropertyHandlingIgnore()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.SetReadOnlyPropertyHandling(ReadOnlyPropertyHandling.Ignore);

            const string json = @"{""MyClass"":{""MyKey"":{""MyInt32"":12}}}";
            var obj = JsonSerializer.Deserialize<SimpleTestClassWithReadOnlyDictionaryProperty>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.MyClass);
            Assert.Equal(0, obj.MyClass.Count);
        }
    }
}
