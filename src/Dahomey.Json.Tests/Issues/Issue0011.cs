using Dahomey.Json.Util;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0011
    {
        public class MyModel
        {
            public ReadOnlyMemory<char> Name { get; set; }
        }

        public class ReadOnlyMemoryOfCharConverter : JsonConverter<ReadOnlyMemory<char>>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return base.CanConvert(typeToConvert);
            }

            public override ReadOnlyMemory<char> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                ReadOnlySpan<byte> rawString = reader.GetRawString();

                int charCount = Encoding.UTF8.GetCharCount(rawString);
                Memory<char> chars = new Memory<char>(new char[charCount]);
                Encoding.UTF8.GetChars(rawString, chars.Span);
                return chars;
            }

            public override void Write(Utf8JsonWriter writer, ReadOnlyMemory<char> value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.Span);
            }
        }

        [Fact]
        public void ReadReadOnlyMemoryOfChar()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.Converters.Insert(0, new ReadOnlyMemoryOfCharConverter());

            const string json = @"{""Name"":""John""}";
            var obj = JsonSerializer.Deserialize<MyModel>(json, options);

            Assert.NotNull(obj);
            Assert.Equal("John", new string(obj.Name.Span));
        }

        [Fact]
        public void WriteReadOnlyMemoryOfChar()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            options.Converters.Insert(0, new ReadOnlyMemoryOfCharConverter());

            const string json = @"{""Name"":""John""}";
            var obj = new MyModel
            {
                Name = "John".AsMemory()
            };

            Helper.TestWrite(obj, json, options);
        }
    }
}
