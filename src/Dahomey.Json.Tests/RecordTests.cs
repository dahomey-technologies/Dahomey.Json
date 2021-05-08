using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests
{
    public class RecordTests
    {
#if NET5_0
        public record Product
        {
            public string Name { get; init; }
            public int Id { get; init; }
        }

        public record InheritedProduct : Product
        {
            public bool IsActive { get; init; }
        }

        public class ClassWithRecord
        {
            public Product Product { get; set; }
        }

        [Fact]
        public void ReadRecord()
        {
            const string json = @"{""Name"":""Foo"",""Id"":12}";
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            Product obj = JsonSerializer.Deserialize<Product>(json, options);

            Assert.NotNull(obj);
            Assert.Equal("Foo", obj.Name);
            Assert.Equal(12, obj.Id);
        }

        [Fact]
        public void ReadInheritedRecord()
        {
            const string json = @"{""Name"":""Foo"",""Id"":12,""IsActive"":true}";
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            InheritedProduct obj = JsonSerializer.Deserialize<InheritedProduct>(json, options);

            Assert.NotNull(obj);
            Assert.Equal("Foo", obj.Name);
            Assert.Equal(12, obj.Id);
            Assert.True(obj.IsActive);
        }

        [Fact]
        public void ReadClassWithRecord()
        {
            const string json = @"{""Product"":{""Name"":""Foo"",""Id"":12}}";
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            ClassWithRecord obj = JsonSerializer.Deserialize<ClassWithRecord>(json, options);

            Assert.NotNull(obj);
            Assert.NotNull(obj.Product);
            Assert.Equal("Foo", obj.Product.Name);
            Assert.Equal(12, obj.Product.Id);
        }

        [Fact]
        public void WriteRecord()
        {
            const string json = @"{""Name"":""Foo"",""Id"":12}";
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            Product obj = new Product { Name = "Foo", Id = 12 };

            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void WriteInheritedRecord()
        {
            const string json = @"{""IsActive"":true,""Name"":""Foo"",""Id"":12}";
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            InheritedProduct obj = new InheritedProduct { Name = "Foo", Id = 12, IsActive = true };

            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void WriteClassWithRecord()
        {
            const string json = @"{""Product"":{""Name"":""Foo"",""Id"":12}}";
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();
            ClassWithRecord obj = new ClassWithRecord { Product = new Product { Name = "Foo", Id = 12 } };

            Helper.TestWrite(obj, json, options);
        }
#endif
    }
}
