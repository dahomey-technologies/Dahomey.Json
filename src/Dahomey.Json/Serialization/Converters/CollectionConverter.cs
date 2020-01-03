using System.Collections.Generic;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters
{
    public class CollectionConverter<TC, TI> : AbstractCollectionConverter<TC, TI>
        where TC : class, ICollection<TI>, new()
    {
        public CollectionConverter(JsonSerializerOptions options)
            : base(options)
        {
        }

        protected override ICollection<TI> InstantiateWorkingCollection()
        {
            return new TC();
        }

        protected override TC InstantiateCollection(ICollection<TI> workingCollection)
        {
            return (TC)workingCollection;
        }
    }
}
