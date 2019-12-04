using System.Collections.Generic;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters
{
    public class InterfaceDictionaryConverter<TK, TV> : AbstractDictionaryConverter<IDictionary<TK, TV>, TK, TV>
    {
        public InterfaceDictionaryConverter(JsonSerializerOptions options)
            : base(options)
        {
        }

        protected override IDictionary<TK, TV> InstantiateWorkingCollection()
        {
            return new Dictionary<TK, TV>();
        }

        protected override IDictionary<TK, TV> InstantiateCollection(IDictionary<TK, TV> workingCollection)
        {
            return workingCollection;
        }
    }
}
