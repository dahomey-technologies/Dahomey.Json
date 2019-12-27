using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Dahomey.Json.Util
{
    public delegate TP StructMemberGetterDelegate<T, TP>(ref T instance);
    public delegate void StructMemberSetterDelegate<T, TP>(ref T instance, TP value);

    public static class FieldInfoExtensions
    {
        public static Func<T, TP> GenerateGetter<T, TP>(this FieldInfo fieldInfo)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T), "instance");
            return Expression.Lambda<Func<T, TP>>(
                Expression.Field(fieldInfo.IsStatic ? null : instanceParam, fieldInfo),
                instanceParam).Compile();
        }

        public static Action<T, TP> GenerateSetter<T, TP>(this FieldInfo fieldInfo)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T), "instance");
            ParameterExpression valueParam = Expression.Parameter(typeof(TP), "value");

            return Expression.Lambda<Action<T, TP>>(
                Expression.Assign(
                    Expression.Field(
                        instanceParam,
                        fieldInfo),
                    valueParam),
                instanceParam, valueParam).Compile();
        }

        public static StructMemberGetterDelegate<T, TP> GenerateStructGetter<T, TP>(this FieldInfo fieldInfo)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T).MakeByRefType(), "instance");
            return Expression.Lambda<StructMemberGetterDelegate<T, TP>>(
                Expression.Field(fieldInfo.IsStatic ? null : instanceParam, fieldInfo),
                instanceParam).Compile();
        }

        public static StructMemberSetterDelegate<T, TP> GenerateStructSetter<T, TP>(this FieldInfo fieldInfo)
        {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T).MakeByRefType(), "instance");
            ParameterExpression valueParam = Expression.Parameter(typeof(TP), "value");

            return Expression.Lambda<StructMemberSetterDelegate<T, TP>>(
                Expression.Assign(
                    Expression.Field(
                        instanceParam,
                        fieldInfo),
                    valueParam),
                instanceParam, valueParam).Compile();
        }
    }
}
