using System.Collections.Generic;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters
{
    public class ArrayConverter<TI> : AbstractCollectionConverter<TI[], TI>
    {
        public ArrayConverter(JsonSerializerOptions options)
            : base(options)
        {
        }

        protected override TI[] InstantiateCollection(ICollection<TI> workingCollection)
        {
            return ((List<TI>)workingCollection).ToArray();
        }

        protected override ICollection<TI> InstantiateWorkingCollection()
        {
            return new List<TI>();
        }
    }
}
