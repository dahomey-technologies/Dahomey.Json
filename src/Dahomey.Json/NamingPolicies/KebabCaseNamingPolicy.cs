using System.Text.Json;

namespace Dahomey.Json.NamingPolicies
{
    public sealed class KebabCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name.ToSeparatedCase('-');
        }
    }
}