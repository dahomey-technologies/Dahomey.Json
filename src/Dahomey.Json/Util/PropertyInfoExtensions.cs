using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Dahomey.Json.Util
{
    public static class PropertyInfoExtensions
    {
        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetAccessors(true)[0].IsStatic;
        }

        public static Func<T, TP> GenerateGetter<T, TP>(this PropertyInfo propertyInfo)
        {
            if (typeof(T).IsClass)
            {
                return (Func<T, TP>)propertyInfo.GetMethod.CreateDelegate(typeof(Func<T, TP>));
            }

            ParameterExpression objParam = Expression.Parameter(typeof(T), "obj");
            return Expression.Lambda<Func<T, TP>>(
                Expression.Property(propertyInfo.IsStatic() ? null : objParam, propertyInfo),
                objParam).Compile();
        }

        public static Action<T, TP> GenerateSetter<T, TP>(this PropertyInfo propertyInfo)
        {
            if (typeof(T).IsClass)
            {
                return (Action<T, TP>)propertyInfo.SetMethod.CreateDelegate(typeof(Action<T, TP>));
            }

            ParameterExpression objParam = Expression.Parameter(typeof(T), "obj");
            ParameterExpression valueParam = Expression.Parameter(typeof(TP), "value");

            return Expression.Lambda<Action<T, TP>>(
                Expression.Assign(
                    Expression.Property(
                        propertyInfo.IsStatic() ? null : objParam,
                        propertyInfo),
                    valueParam),
                objParam, valueParam).Compile();
        }
    }
}
