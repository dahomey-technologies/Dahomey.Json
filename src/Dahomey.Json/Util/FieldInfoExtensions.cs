using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Dahomey.Json.Util
{
    public static class FieldInfoExtensions
    {
        public static Func<T, TP> GenerateGetter<T, TP>(this FieldInfo fieldInfo)
        {
            ParameterExpression objParam = Expression.Parameter(typeof(T), "obj");
            return Expression.Lambda<Func<T, TP>>(
                Expression.Field(fieldInfo.IsStatic ? null : objParam, fieldInfo),
                objParam).Compile();
        }

        public static Action<T, TP> GenerateSetter<T, TP>(this FieldInfo fieldInfo)
        {
            ParameterExpression objParam = Expression.Parameter(typeof(T), "obj");
            ParameterExpression valueParam = Expression.Parameter(typeof(TP), "value");

            return Expression.Lambda<Action<T, TP>>(
                Expression.Assign(
                    Expression.Field(
                        objParam,
                        fieldInfo),
                    valueParam),
                objParam, valueParam).Compile();
        }
    }
}
