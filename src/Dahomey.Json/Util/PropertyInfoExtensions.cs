using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

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
            if (propertyInfo.GetMethod == null)
            {
                throw new JsonException("Unexpected");
            }

            return (Func<T, TP>)propertyInfo.GetMethod.CreateDelegate(typeof(Func<T, TP>));
        }

        public static Action<T, TP> GenerateSetter<T, TP>(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.SetMethod == null)
            {
                throw new JsonException("Unexpected");
            }

            return (Action<T, TP>)propertyInfo.SetMethod.CreateDelegate(typeof(Action<T, TP>));
        }

        public static StructMemberGetterDelegate<T, TP> GenerateStructGetter<T, TP>(this PropertyInfo propertyInfo)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T).MakeByRefType(), "instance");
            return Expression.Lambda<StructMemberGetterDelegate<T, TP>>(
                Expression.Property(propertyInfo.IsStatic() ? null : instanceParam, propertyInfo),
                instanceParam).Compile();
        }

        public static StructMemberSetterDelegate<T, TP> GenerateStructSetter<T, TP>(this PropertyInfo propertyInfo)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T).MakeByRefType(), "instance");
            ParameterExpression valueParam = Expression.Parameter(typeof(TP), "value");

            return Expression.Lambda<StructMemberSetterDelegate<T, TP>>(
                Expression.Assign(
                    Expression.Property(
                        propertyInfo.IsStatic() ? null : instanceParam,
                        propertyInfo),
                    valueParam),
                instanceParam, valueParam).Compile();
        }
    }
}
