using System.Collections.Generic;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters
{
    public class DictionaryConverter<TC, TK, TV> : AbstractDictionaryConverter<TC, TK, TV>
        where TC : class, IDictionary<TK, TV>, new()
    {
        public DictionaryConverter(JsonSerializerOptions options)
            : base(options)
        {
        }

        protected override IDictionary<TK, TV> InstantiateWorkingCollection()
        {
            return new TC();
        }

        protected override TC InstantiateCollection(IDictionary<TK, TV> workingCollection)
        {
            return (TC)workingCollection;
        }
    }
}
