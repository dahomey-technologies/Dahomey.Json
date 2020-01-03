using System;
using System.Text.Json;

namespace Dahomey.Json.NamingPolicies
{
    public sealed class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name.ToSeparatedCase('_');
        }
    }
}