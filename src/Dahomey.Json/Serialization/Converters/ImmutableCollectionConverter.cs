using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters
{
    public class ImmutableCollectionConverter<TC, TI> : AbstractCollectionConverter<TC, TI>
        where TC : ICollection<TI>
    {
        private readonly Func<IEnumerable<TI>, TC> _createRangeDelegate;

        public ImmutableCollectionConverter(JsonSerializerOptions options)
            : base(options)
        {
            string typeFullName = typeof(TC).GetGenericTypeDefinition().FullName;
            string staticTypeFullName = typeFullName.Substring(0, typeFullName.Length - 2);
            Assembly assembly = typeof(TC).Assembly;
            Type type = assembly.GetType(staticTypeFullName);
            MethodInfo methodInfo = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.IsGenericMethod && m.GetGenericMethodDefinition().Name == "CreateRange")
                .MakeGenericMethod(typeof(TI));
            _createRangeDelegate = (Func<IEnumerable<TI>, TC>)Delegate.CreateDelegate(typeof(Func<IEnumerable<TI>, TC>), methodInfo);
        }

        protected override ICollection<TI> InstantiateWorkingCollection()
        {
            return new List<TI>();
        }

        protected override TC InstantiateCollection(ICollection<TI> workingCollection)
        {
            return _createRangeDelegate(workingCollection);
        }
    }
}
