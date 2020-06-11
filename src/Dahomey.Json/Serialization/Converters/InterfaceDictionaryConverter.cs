using System.Collections.Generic;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters
{
    public class InterfaceDictionaryConverter<TI, TK, TV> : AbstractDictionaryConverter<TI, TK, TV>
        where TK : notnull
        where TI : IEnumerable<KeyValuePair<TK, TV>>

    {
        public InterfaceDictionaryConverter(JsonSerializerOptions options)
            : base(options)
        {
        }

        protected override IDictionary<TK, TV> InstantiateWorkingCollection()
        {
            return new Dictionary<TK, TV>();
        }

        protected override TI InstantiateCollection(IDictionary<TK, TV> workingCollection)
        {
            return (TI)workingCollection;
        }
    }
}
