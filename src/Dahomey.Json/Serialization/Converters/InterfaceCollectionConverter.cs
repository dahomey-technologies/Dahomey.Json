using System.Collections.Generic;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters
{
    public class InterfaceCollectionConverter<TC, TF, TI> : AbstractCollectionConverter<TF, TI>
        where TF : ICollection<TI>
        where TC : class, TF, new()
    {
        public InterfaceCollectionConverter(JsonSerializerOptions options)
            : base(options)
        {
        }

        protected override ICollection<TI> InstantiateWorkingCollection()
        {
            return new TC();
        }

        protected override TF InstantiateCollection(ICollection<TI> workingCollection)
        {
            return (TF)workingCollection;
        }
    }
}
