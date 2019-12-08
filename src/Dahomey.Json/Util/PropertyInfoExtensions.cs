using System;
using System.Reflection;

namespace Dahomey.Json.Util
{
    public static class PropertyInfoExtensions
    {
        public static Func<T, TP> GenerateGetter<T, TP>(this PropertyInfo propertyInfo)
        {
            return (Func<T, TP>)propertyInfo.GetMethod.CreateDelegate(typeof(Func<T, TP>));
        }

        public static Action<T, TP> GenerateSetter<T, TP>(this PropertyInfo propertyInfo)
        {
            return (Action<T, TP>)propertyInfo.SetMethod.CreateDelegate(typeof(Action<T, TP>));
        }
    }
}
