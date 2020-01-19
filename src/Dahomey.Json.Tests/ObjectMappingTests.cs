using Xunit;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Dahomey.Json.Serialization.Converters.Mappings;
using System.Text.Json.Serialization;
using Dahomey.Json.Serialization.Conventions;

namespace Dahomey.Json.Tests
{
    public class ObjectMappingTests
    {
        public enum EnumTest
        {
            None = 0,
            Value1 = 1,
            Value2 = 2
        }

        public class SimpleObject
        {
            public bool Boolean { get; set; }
            public sbyte SByte { get; set; }
            public byte Byte { get; set; }
            public ushort Int16 { get; set; }
            public short UInt16 { get; set; }
            public int Int32 { get; set; }
            public uint UInt32 { get; set; }
            public long Int64 { get; set; }
            public ulong UInt64 { get; set; }
            public string String { get; set; }
            public float Single { get; set; }
            public double Double { get; set; }
            public DateTime DateTime { get; set; }
            public EnumTest Enum { get; set; }
        }

        public class IntObject
        {
            public int IntValue { get; set; }

            protected bool Equals(IntObject other)
            {
                return IntValue == other.IntValue;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((IntObject)obj);
            }

            public override int GetHashCode()
            {
                return IntValue;
            }
        }

        [Fact]
        public void ReadWithMapMember()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<SimpleObject>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .ClearMemberMappings()
                    .MapMember(o => o.Boolean)
            );

            const string json = @"{""Boolean"":true,""SByte"":13,""Byte"":12,""Int16"":14,""UInt16"":15,""Int32"":16,""UInt32"":17,""Int64"":18,""UInt64"":19,""String"":""string"",""Single"":20.209999084472656,""Double"":22.23,""DateTime"":""2014-02-21T19:00:00Z"",""Enum"":""Value1""}";
            SimpleObject obj = Helper.Read<SimpleObject>(json, options);

            Assert.NotNull(obj);
            Assert.True(obj.Boolean);
            Assert.Equal(0, obj.Byte);
            Assert.Equal(0, obj.SByte);
            Assert.Equal(0, obj.Int16);
            Assert.Equal(0, obj.UInt16);
            Assert.Equal(0, obj.Int32);
            Assert.Equal(0u, obj.UInt32);
            Assert.Equal(0L, obj.Int64);
            Assert.Equal(0ul, obj.UInt64);
            Assert.Equal(0f, obj.Single);
            Assert.Equal(0.0, obj.Double);
            Assert.Null(obj.String);
            Assert.Equal(DateTime.MinValue, obj.DateTime);
            Assert.Equal(EnumTest.None, obj.Enum);
        }

        [Fact]
        public void WriteWithMapMember()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<SimpleObject>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .ClearMemberMappings()
                    .MapMember(o => o.Boolean)
            );

            SimpleObject obj = new SimpleObject
            {
                Boolean = true,
                Byte = 12,
                SByte = 13,
                Int16 = 14,
                UInt16 = 15,
                Int32 = 16,
                UInt32 = 17u,
                Int64 = 18,
                UInt64 = 19ul,
                Single = 20.21f,
                Double = 22.23,
                String = "string",
                DateTime = new DateTime(2014, 02, 21, 19, 0, 0, DateTimeKind.Utc),
                Enum = EnumTest.Value1
            };

            const string json = @"{""Boolean"":true}";
            Helper.TestWrite(obj, json, options);
        }

        class ObjectWithGuid
        {
            public Guid Guid { get; set; }
        }

        [Fact]
        public void ReadWithMemberNameAndConverter()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<ObjectWithGuid>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .ClearMemberMappings()
                    .MapMember(o => o.Guid)
                        .SetConverter(new GuidConverter())
                        .SetMemberName("g")
            );

            const string json = @"{""g"":""e933aae2d74942aeb8628ac805df082f""}";
            ObjectWithGuid obj = Helper.Read<ObjectWithGuid>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(Guid.Parse("E933AAE2-D749-42AE-B862-8AC805DF082F"), obj.Guid);
        }

        [Fact]
        public void WriteWithMemberNameAndConverter()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<ObjectWithGuid>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .ClearMemberMappings()
                    .MapMember(o => o.Guid)
                        .SetConverter(new GuidConverter())
                        .SetMemberName("g")
            );

            ObjectWithGuid obj = new ObjectWithGuid
            {
                Guid = Guid.Parse("E933AAE2-D749-42AE-B862-8AC805DF082F")
            };

            const string json = @"{""g"":""e933aae2d74942aeb8628ac805df082f""}";
            Helper.TestWrite(obj, json, options);
        }

        [Fact]
        public void ReadWithNamingPolicy()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<IntObject>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .SetPropertyNamingPolicy(JsonNamingPolicy.CamelCase)
            );

            const string json = @"{""intValue"":12}";
            IntObject obj = Helper.Read<IntObject>(json, options);

            Assert.NotNull(obj);
            Assert.Equal(12, obj.IntValue);
        }

        [Fact]
        public void WriteWithNamingPolicy()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingRegistry().Register<IntObject>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .SetPropertyNamingPolicy(JsonNamingPolicy.CamelCase)
            );

            IntObject obj = new IntObject
            {
                IntValue = 12
            };

            const string json = @"{""intValue"":12}";
            Helper.TestWrite(obj, json, options);
        }

        public class BaseObject
        {
            public int BaseValue { get; set; }
        }

        public class InheritedObject : BaseObject
        {
            public int InheritedValue { get; set; }
        }

        [Fact]
        public void ReadWithDiscriminator()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetDiscriminatorConventionRegistry().ClearConventions();
            options.GetDiscriminatorConventionRegistry().RegisterConvention(
                new DefaultDiscriminatorConvention<string>(options, "_t"));
            options.GetObjectMappingRegistry().Register<InheritedObject>(objectMapping =>
                objectMapping
                    .AutoMap()
                    .SetDiscriminator("inherited")
            );

            const string json = @"{""_t"":""inherited"",""InheritedValue"":13,""BaseValue"":12}";
            BaseObject obj = Helper.Read<BaseObject>(json, options);

            Assert.NotNull(obj);
            Assert.IsType<InheritedObject>(obj);
            Assert.Equal(12, obj.BaseValue);
            Assert.Equal(13, ((InheritedObject)obj).InheritedValue);
        }

        [Fact]
        public void WriteWithDiscriminator()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetDiscriminatorConventionRegistry().ClearConventions();
            options.GetDiscriminatorConventionRegistry().RegisterConvention(
                new DefaultDiscriminatorConvention<string>(options, "_t"));
            options.GetObjectMappingRegistry().Register<InheritedObject>(objectMapping =>
                objectMapping
                    .SetDiscriminator("inherited")
                    .AutoMap()
            );

            InheritedObject obj = new InheritedObject
            {
                BaseValue = 12,
                InheritedValue = 13
            };

            const string json = @"{""_t"":""inherited"",""InheritedValue"":13,""BaseValue"":12}";
            Helper.TestWrite<BaseObject>(obj, json,  options);
        }

        public class OptInObjectMappingConventionProvider : IObjectMappingConventionProvider
        {
            public IObjectMappingConvention GetConvention(Type type)
            {
                // here you could filter which type should be optIn and return null for other types
                return new OptInObjectMappingConvention();
            }
        }

        public class OptInObjectMappingConvention : IObjectMappingConvention
        {
            private readonly DefaultObjectMappingConvention _defaultConvention = new DefaultObjectMappingConvention();

            public void Apply<T>(JsonSerializerOptions options, ObjectMapping<T> objectMapping)
            {
                _defaultConvention.Apply(options, objectMapping);

                // restrict to members holding JsonPropertyNameAttribute
                objectMapping.SetMemberMappings(objectMapping.MemberMappings
                    .Where(m => m.MemberInfo != null && m.MemberInfo.IsDefined(typeof(JsonPropertyNameAttribute)))
                    .ToList());
            }
        }

        public class OptInObject1
        {
            [JsonPropertyName("Id")]
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public class OptInObject2
        {
            public int Id { get; set; }

            [JsonPropertyName("Name")]
            public string Name { get; set; }
        }

        [Fact]
        public void OptIn()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();
            options.GetObjectMappingConventionRegistry().RegisterProvider(
                new OptInObjectMappingConventionProvider()
            );

            OptInObject1 obj1 = new OptInObject1 { Id = 12, Name = "foo" };
            const string json1 = @"{""Id"":12}";
            Helper.TestWrite(obj1, json1, options);

            OptInObject2 obj2 = new OptInObject2 { Id = 12, Name = "foo" };
            const string json2 = @"{""Name"":""foo""}";
            Helper.TestWrite(obj2, json2, options);
        }
    }
}
