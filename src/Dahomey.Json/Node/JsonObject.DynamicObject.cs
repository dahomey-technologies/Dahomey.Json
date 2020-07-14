using System.Dynamic;

namespace System.Text.Json
{
    public sealed partial class JsonObject
    {
        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            if (!TryGetPropertyValue(binder.Name, out JsonNode? jsonNode))
            {
                return base.TryGetMember(binder, out result);
            }

            result = jsonNode;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            if (value == null)
            {
                this[binder.Name] = JsonNull.Instance;
                return true;
            }

            switch (value)
            {
                case sbyte sByteValue:
                    this[binder.Name] = sByteValue;
                    return true;

                case byte byteValue:
                    this[binder.Name] = byteValue;
                    return true;

                case short shortValue:
                    this[binder.Name] = shortValue;
                    return true;

                case ushort ushortValue:
                    this[binder.Name] = ushortValue;
                    return true;

                case int intValue:
                    this[binder.Name] = intValue;
                    return true;

                case uint uintValue:
                    this[binder.Name] = uintValue;
                    return true;

                case long longValue:
                    this[binder.Name] = longValue;
                    return true;

                case ulong ulongValue:
                    this[binder.Name] = ulongValue;
                    return true;

                case decimal decimalValue:
                    this[binder.Name] = decimalValue;
                    return true;

                case float singleValue:
                    this[binder.Name] = singleValue;
                    return true;

                case double doubleValue:
                    this[binder.Name] = doubleValue;
                    return true;

                case bool boolValue:
                    this[binder.Name] = boolValue;
                    return true;

                case string stringValue:
                    this[binder.Name] = stringValue;
                    return true;

                case JsonNode jsonNode:
                    this[binder.Name] = jsonNode;
                    return true;

                case int[] intArray:
                    this[binder.Name] = new JsonArray(intArray);
                    return true;
            }

            return base.TrySetMember(binder, value);
        }
    }
}
