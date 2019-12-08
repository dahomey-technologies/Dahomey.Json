using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class CallbackTests
    {
        private class ObjectWithCallbacks
        {
            public int Id { get; set; }

            [JsonIgnore] public bool OnDeserializingCalled { get; private set; }
            [JsonIgnore] public bool OnDeserializedCalled { get; private set; }
            [JsonIgnore] public bool OnSerializingCalled { get; private set; }
            [JsonIgnore] public bool OnSerializedCalled { get; private set; }

            [OnDeserializing]
            public void OnDeserializing()
            {
                Assert.False(OnDeserializingCalled);
                Assert.False(OnDeserializedCalled);
                Assert.Equal(0, Id);
                OnDeserializingCalled = true;
            }

            [OnDeserialized]
            public void OnDeserialized()
            {
                Assert.True(OnDeserializingCalled);
                Assert.False(OnDeserializedCalled);
                OnDeserializedCalled = true;
            }

            [OnDeserializing]
            public void OnSerializing()
            {
                Assert.False(OnSerializingCalled);
                Assert.False(OnSerializedCalled);
                OnSerializingCalled = true;
            }

            [OnDeserialized]
            public void OnSerialized()
            {
                Assert.True(OnSerializingCalled);
                Assert.False(OnSerializedCalled);
                OnSerializedCalled = true;
            }
        }

        [Fact]
        public void TestReadByAttribute()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Id"":12}";
            ObjectWithCallbacks obj = Helper.Read<ObjectWithCallbacks>(json, options);

            Assert.NotNull(obj);
            Assert.True(obj.OnDeserializingCalled);
            Assert.True(obj.OnDeserializedCalled);
            Assert.False(obj.OnSerializingCalled);
            Assert.False(obj.OnSerializedCalled);
        }

        [Fact]
        public void TestWriteByAttribute()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            const string json = @"{""Id"":12}";
            Helper.TestWrite(new ObjectWithCallbacks { Id = 12 }, json, options);
        }

        [Fact]
        public void TestReadByApi()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<ObjectWithCallbacks>(om =>
            {
                om.MapMember(o => o.Id);
                om.SetOnDeserializingMethod(o => o.OnDeserializing());
                om.SetOnDeserializedMethod(o => o.OnDeserialized());
            });

            const string json = @"{""Id"":12}";
            ObjectWithCallbacks obj = Helper.Read<ObjectWithCallbacks>(json, options);

            Assert.NotNull(obj);
            Assert.True(obj.OnDeserializingCalled);
            Assert.True(obj.OnDeserializedCalled);
            Assert.False(obj.OnSerializingCalled);
            Assert.False(obj.OnSerializedCalled);
        }

        [Fact]
        public void TestWriteByApi()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<ObjectWithCallbacks>(om =>
            {
                om.MapMember(o => o.Id);
                om.SetOnSerializingMethod(o => o.OnSerializing());
                om.SetOnSerializedMethod(o => o.OnSerialized());
            });

            const string json = @"{""Id"":12}";
            Helper.TestWrite(new ObjectWithCallbacks { Id = 12 }, json, options);
        }
    }
}
