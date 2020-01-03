using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    public class Issue0004
    {
        public sealed class SignatureDefinition
        {
            public string Definition { get; set; }
        }

        public sealed class SignedDocument
        {
            public SignedDocument(string id, string name, IReadOnlyCollection<SignatureDefinition> signatureDefinitions)
            {
                Id = id;
                Name = name;
                SignatureDefinitions = signatureDefinitions;
            }

            public string Id { get; }
            public string Name { get; }
            public IReadOnlyCollection<SignatureDefinition> SignatureDefinitions { get; }
        }

        [Fact]
        public void TestReadIReadOnlyCollection()
        {
            JsonSerializerOptions options = new JsonSerializerOptions().SetupExtensions();

            const string json = @"{""Id"":""ABCD"",""Name"":""Doc"",""SignatureDefinitions"":[{""Definition"":""Def1""},{""Definition"":""Def2""}]}";
            var obj = JsonSerializer.Deserialize<SignedDocument>(json, options);

            Assert.NotNull(obj);
            Assert.Equal("ABCD", obj.Id);
            Assert.Equal("Doc", obj.Name);
            Assert.NotNull(obj.SignatureDefinitions);
            Assert.IsType<List<SignatureDefinition>>(obj.SignatureDefinitions);
            Assert.Equal(2, obj.SignatureDefinitions.Count);
            Assert.Equal("Def1", obj.SignatureDefinitions.ElementAt(0).Definition);
            Assert.Equal("Def2", obj.SignatureDefinitions.ElementAt(1).Definition);
        }
    }
}
