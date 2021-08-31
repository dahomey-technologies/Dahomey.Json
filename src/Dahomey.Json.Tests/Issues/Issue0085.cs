using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace Dahomey.Json.Tests.Issues
{
    /// <summary>
    /// Discriminator added more than once when serializing multi-level inheritance classes
    /// https://github.com/dahomey-technologies/Dahomey.Json/issues/85
    /// </summary>
    public class Issue0085
    {
        private abstract class BaseClass
        {
            public int Id { get; set; }
        }

        [JsonDiscriminator(nameof(DerivedClassLevelOne))]
        private class DerivedClassLevelOne : BaseClass
        {
            public string LevelOne { get; set; }
        }

        [JsonDiscriminator(nameof(DerivedClassLevelTwoOne))]
        private class DerivedClassLevelTwoOne : DerivedClassLevelOne
        {
            public int LevelTwoOne { get; set; }
        }

        [JsonDiscriminator(nameof(DerivedClassLevelTwoTwo))]
        private class DerivedClassLevelTwoTwo : DerivedClassLevelOne
        {
            public int LevelTwoTwo { get; set; }
        }

        [Fact]
        public void ShouldAddDiscriminatorToJsonOnce()
        {
            JsonSerializerOptions options = new();
            options.SetupExtensions();
            DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
            registry.RegisterType<DerivedClassLevelOne>();
            registry.RegisterType<DerivedClassLevelTwoOne>();
            registry.RegisterType<DerivedClassLevelTwoTwo>();

            registry.DiscriminatorPolicy = DiscriminatorPolicy.Always;

            var ic1 = new DerivedClassLevelOne // Mid-level
            {
                LevelOne = "Some value",
                Id = 1
            };

            string actual = Helper.Write(ic1, options);
            const string expected = @"{""$type"":""DerivedClassLevelOne"",""LevelOne"":""Some value"",""Id"":1}";
            Assert.Equal(expected, actual); // There should only be one $type key in the json
        }
    }
}
