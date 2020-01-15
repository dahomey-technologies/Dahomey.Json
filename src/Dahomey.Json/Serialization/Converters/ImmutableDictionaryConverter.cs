using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters
{
    public class ImmutableDictionaryConverter<TC, TK, TV> : AbstractDictionaryConverter<TC, TK, TV>
        where TK : notnull
        where TC : IDictionary<TK, TV>
    {
        private readonly Func<IEnumerable<KeyValuePair<TK, TV>>, TC> _createRangeDelegate;

        public ImmutableDictionaryConverter(JsonSerializerOptions options)
            : base(options)
        {
            string? typeFullName = typeof(TC).GetGenericTypeDefinition().FullName;

            if (typeFullName == null)
            {
                throw new JsonException($"Cannot find {typeof(TC)} full name");
            }

            string staticTypeFullName = typeFullName.Substring(0, typeFullName.Length - 2);
            Assembly assembly = typeof(TC).Assembly;
            Type? type = assembly.GetType(staticTypeFullName);

            if (type == null)
            {
                throw new JsonException($"Cannot find type from {staticTypeFullName}");
            }

            MethodInfo methodInfo = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.IsGenericMethod && m.GetGenericMethodDefinition().Name == "CreateRange")
                .MakeGenericMethod(typeof(TK), typeof(TV));
            _createRangeDelegate = (Func<IEnumerable<KeyValuePair<TK, TV>>, TC>)Delegate.CreateDelegate(typeof(Func<IEnumerable<KeyValuePair<TK, TV>>, TC>), methodInfo);
        }

        protected override IDictionary<TK, TV> InstantiateWorkingCollection()
        {
            return new Dictionary<TK, TV>();
        }

        protected override TC InstantiateCollection(IDictionary<TK, TV> workingCollection)
        {
            return _createRangeDelegate(workingCollection);
        }
    }
}
