using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters
{
    public class ReadOnlyDictionaryConverter<TK, TV> : AbstractDictionaryConverter<ReadOnlyDictionary<TK, TV>, TK, TV>
        where TK : notnull
    {
        public ReadOnlyDictionaryConverter(JsonSerializerOptions options)
            : base(options)
        {
        }

        protected override IDictionary<TK, TV> InstantiateWorkingCollection()
        {
            return new Dictionary<TK, TV>();
        }

        protected override ReadOnlyDictionary<TK, TV> InstantiateCollection(IDictionary<TK, TV> workingCollection)
        {
            return new ReadOnlyDictionary<TK, TV>(workingCollection);
        }
    }
}
