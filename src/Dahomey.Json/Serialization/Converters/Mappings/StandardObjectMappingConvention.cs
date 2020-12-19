using Dahomey.Json.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class StandardObjectMappingConvention : IObjectMappingConvention
    {
        public void Apply<T>(JsonSerializerOptions options, ObjectMapping<T> objectMapping)
        {
            Type type = objectMapping.ObjectType;
            List<MemberMapping<T>> memberMappings = new List<MemberMapping<T>>();

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (PropertyInfo propertyInfo in properties)
            {
                if (!propertyInfo.IsDefined(typeof(DataMemberAttribute)))
                {
                    continue;
                }

                if (typeof(Delegate).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    continue;
                }

                MemberMapping<T> memberMapping = new MemberMapping<T>(options, objectMapping, propertyInfo, propertyInfo.PropertyType);
                ProcessDefaultValue(propertyInfo, memberMapping);
                ProcessShouldSerializeMethod(memberMapping);
                ProcessRequired(propertyInfo, memberMapping);
                ProcessMemberName(propertyInfo, memberMapping);
                memberMappings.Add(memberMapping);
            }

            foreach (FieldInfo fieldInfo in fields)
            {
                if (!fieldInfo.IsDefined(typeof(DataMemberAttribute)))
                {
                    continue;
                }

                Type fieldType = fieldInfo.FieldType;

                if (typeof(Delegate).IsAssignableFrom(fieldType))
                {
                    continue;
                }

                MemberMapping<T> memberMapping = new MemberMapping<T>(options, objectMapping, fieldInfo, fieldInfo.FieldType);
                ProcessDefaultValue(fieldInfo, memberMapping);
                ProcessShouldSerializeMethod(memberMapping);
                ProcessRequired(fieldInfo, memberMapping);
                ProcessMemberName(fieldInfo, memberMapping);

                memberMappings.Add(memberMapping);
            }

            objectMapping.AddMemberMappings(memberMappings);

            ConstructorInfo[] constructorInfos = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            // if no default constructor, pick up first one
            if (constructorInfos.Length > 0 && !constructorInfos.Any(c => c.GetParameters().Length == 0))
            {
                objectMapping.MapCreator(constructorInfos[0]);
            }

            MethodInfo? methodInfo = type.GetMethods()
                .FirstOrDefault(m => m.IsDefined(typeof(OnDeserializingAttribute)));
            if (methodInfo != null)
            {
                objectMapping.SetOnDeserializingMethod(GenerateCallbackDelegate<T>(methodInfo));
            }
            else if (type.GetInterfaces().Any(i => i == typeof(ISupportInitialize)))
            {
                objectMapping.SetOnDeserializingMethod(t => ((ISupportInitialize?)t)?.BeginInit());
            }

            methodInfo = type.GetMethods()
                .FirstOrDefault(m => m.IsDefined(typeof(OnDeserializedAttribute)));
            if (methodInfo != null)
            {
                objectMapping.SetOnDeserializedMethod(GenerateCallbackDelegate<T>(methodInfo));
            }
            else if (type.GetInterfaces().Any(i => i == typeof(ISupportInitialize)))
            {
                objectMapping.SetOnDeserializedMethod(t => ((ISupportInitialize?)t)?.EndInit());
            }

            methodInfo = type.GetMethods()
                .FirstOrDefault(m => m.IsDefined(typeof(OnSerializingAttribute)));
            if (methodInfo != null)
            {
                objectMapping.SetOnSerializingMethod(GenerateCallbackDelegate<T>(methodInfo));
            }

            methodInfo = type.GetMethods()
                .FirstOrDefault(m => m.IsDefined(typeof(OnSerializedAttribute)));
            if (methodInfo != null)
            {
                objectMapping.SetOnSerializedMethod(GenerateCallbackDelegate<T>(methodInfo));
            }
        }

        private void ProcessDefaultValue<T>(MemberInfo memberInfo, MemberMapping<T> memberMapping)
        {
            DefaultValueAttribute? defaultValueAttribute = memberInfo.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttribute != null)
            {
                memberMapping.SetDefaultValue(defaultValueAttribute.Value);
            }

            DataMemberAttribute? dataMemberAttribute = memberInfo.GetCustomAttribute<DataMemberAttribute>();
            if (dataMemberAttribute != null && !dataMemberAttribute.EmitDefaultValue)
            {
                memberMapping.SetIngoreIfDefault(true);
            }
        }

        private void ProcessShouldSerializeMethod<T>(MemberMapping<T> memberMapping)
        {
            string shouldSerializeMethodName = "ShouldSerialize" + memberMapping.MemberInfo.Name;
            Type? objectType = memberMapping.MemberInfo.DeclaringType;

            if (objectType == null)
            {
                return;
            }

            MethodInfo? shouldSerializeMethodInfo = objectType.GetMethod(shouldSerializeMethodName, new Type[] { });
            if (shouldSerializeMethodInfo != null &&
                shouldSerializeMethodInfo.IsPublic &&
                shouldSerializeMethodInfo.ReturnType == typeof(bool))
            {
                // obj => ((TClass) obj).ShouldSerializeXyz()
                ParameterExpression objParameter = Expression.Parameter(typeof(object), "obj");
                Expression<Func<object, bool>> lambdaExpression = Expression.Lambda<Func<object, bool>>(
                    Expression.Call(
                        Expression.Convert(objParameter, objectType), 
                        shouldSerializeMethodInfo), 
                    objParameter);

                memberMapping.SetShouldSerializeMethod(lambdaExpression.Compile());
            }
        }

        private void ProcessRequired<T>(MemberInfo memberInfo, MemberMapping<T> memberMapping)
        {
            DataMemberAttribute? dataMemberAttribute = memberInfo.GetCustomAttribute<DataMemberAttribute>();
            if (dataMemberAttribute != null && dataMemberAttribute.IsRequired)
            {
                memberMapping.SetRequired(RequirementPolicy.Always);
            }
        }

        private void ProcessMemberName<T>(MemberInfo memberInfo, MemberMapping<T> memberMapping)
        {
            DataMemberAttribute? dataMemberAttribute = memberInfo.GetCustomAttribute<DataMemberAttribute>();
            if (dataMemberAttribute != null && !string.IsNullOrEmpty(dataMemberAttribute.Name))
            {
                memberMapping.SetMemberName(dataMemberAttribute.Name);
            }
        }

        private Action<T> GenerateCallbackDelegate<T>(MethodInfo methodInfo)
        {
            // obj => obj.Callback()
            ParameterExpression objParameter = Expression.Parameter(typeof(T), "obj");
            Expression<Action<T>> lambdaExpression = Expression.Lambda<Action<T>>(
                    Expression.Call(
                        Expression.Convert(objParameter, typeof(T)),
                        methodInfo),
                objParameter);

            return lambdaExpression.Compile();
        }
    }
}
