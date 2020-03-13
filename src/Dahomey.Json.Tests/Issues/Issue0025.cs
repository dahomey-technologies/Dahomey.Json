using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    /// <summary>
    /// https://github.com/dahomey-technologies/Dahomey.Json/issues/25
    /// </summary>
    public class Issue0025
    {
        public interface IIdentity<TKey> where TKey : notnull
        {
            TKey Id { get; set; }
        }

        public abstract class EntityState<TKey> : IIdentity<TKey> where TKey : notnull
        {
            public TKey Id { get; set; }
        }

        public class SigningDocument
        {
            public string Foo { get; set; }
        }

        public class RequestEntityState : EntityState<string>
        {
            public string FullName { get; set; }
            public IReadOnlyCollection<SigningDocument> Documents { get; set; }
        }

        internal sealed class RequestDB : RequestEntityState
        {
            public bool ShouldSerializeDocuments() { return false; }
        }

        [Fact]
        public void Test()
        {
            RequestDB obj = new RequestDB
            {
                Id = "1234",
                FullName = "Foo",
                Documents = new List<SigningDocument>
                {
                    new SigningDocument
                    {
                        Foo = "Bar"
                    }
                }
            };

            const string json = @"{""FullName"":""Foo"",""Id"":""1234""}";

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.SetupExtensions();

            Helper.TestWrite(obj, json, options);
        }
    }
}
