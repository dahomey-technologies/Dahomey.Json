using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class DefaultObjectMappingConvention : IObjectMappingConvention
    {
        public void Apply<T>(JsonSerializerOptions options, ObjectMapping<T> objectMapping) where T : class
        {
            Type type = objectMapping.ObjectType;
            List<MemberMapping<T>> memberMappings = new List<MemberMapping<T>>();

            JsonDiscriminatorAttribute discriminatorAttribute = type.GetCustomAttribute<JsonDiscriminatorAttribute>();

            if (discriminatorAttribute != null)
            {
                objectMapping.SetDiscriminator(discriminatorAttribute.Discriminator);
                objectMapping.SetDiscriminatorPolicy(discriminatorAttribute.Policy);
            }

            Type namingPolicyType = type.GetCustomAttribute<JsonNamingPolicyAttribute>()?.NamingPolicyType;
            if (namingPolicyType != null)
            {
                objectMapping.SetPropertyNamingPolicy((JsonNamingPolicy)Activator.CreateInstance(namingPolicyType));
            }

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.IsDefined(typeof(JsonIgnoreAttribute)))
                {
                    continue;
                }

                if (propertyInfo.IsDefined(typeof(JsonExtensionDataAttribute)))
                {
                    objectMapping.MapExtensionData(propertyInfo);
                    continue;
                }

                if ((propertyInfo.GetMethod.IsPrivate || propertyInfo.GetMethod.IsStatic) 
                    && !propertyInfo.IsDefined(typeof(JsonPropertyNameAttribute))
                    && !propertyInfo.IsDefined(typeof(JsonPropertyAttribute)))
                {
                    continue;
                }

                MemberMapping<T> memberMapping = new MemberMapping<T>(options, objectMapping, propertyInfo, propertyInfo.PropertyType);
                ProcessDefaultValue(propertyInfo, memberMapping);
                ProcessShouldSerializeMethod(memberMapping);
                memberMappings.Add(memberMapping);
            }

            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.IsDefined(typeof(JsonIgnoreAttribute)))
                {
                    continue;
                }

                if ((fieldInfo.IsPrivate || fieldInfo.IsStatic) 
                    && !fieldInfo.IsDefined(typeof(JsonPropertyNameAttribute))
                    && !fieldInfo.IsDefined(typeof(JsonPropertyAttribute)))
                {
                    continue;
                }

                Type fieldType = fieldInfo.FieldType;

                MemberMapping<T> memberMapping = new MemberMapping<T>(options, objectMapping, fieldInfo, fieldInfo.FieldType);
                ProcessDefaultValue(fieldInfo, memberMapping);
                ProcessShouldSerializeMethod(memberMapping);

                memberMappings.Add(memberMapping);
            }

            objectMapping.AddMemberMappings(memberMappings);

            ConstructorInfo[] constructorInfos = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            ConstructorInfo constructorInfo = constructorInfos
                .FirstOrDefault(c => c.IsDefined(typeof(JsonConstructorAttribute)));

            if (constructorInfo != null)
            {
                JsonConstructorAttribute constructorAttribute = constructorInfo.GetCustomAttribute<JsonConstructorAttribute>();
                CreatorMapping creatorMapping = objectMapping.MapCreator(constructorInfo);
                if (constructorAttribute.MemberNames != null)
                {
                    creatorMapping.SetMemberNames(constructorAttribute.MemberNames);
                }
            }
            // if no default constructor, pick up first one
            else if (constructorInfos.Length > 0 && !constructorInfos.Any(c => c.GetParameters().Length == 0))
            {
                constructorInfo = constructorInfos[0];
                objectMapping.MapCreator(constructorInfo);
            }

            MethodInfo methodInfo = type.GetMethods()
                .FirstOrDefault(m => m.IsDefined(typeof(OnDeserializingAttribute)));
            if (methodInfo != null)
            {
                objectMapping.SetOnDeserializingMethod(GenerateCallbackDelegate<T>(methodInfo));
            }
            else if (type.GetInterfaces().Any(i => i == typeof(ISupportInitialize)))
            {
                objectMapping.SetOnDeserializingMethod(t => ((ISupportInitialize)t).BeginInit());
            }

            methodInfo = type.GetMethods()
                .FirstOrDefault(m => m.IsDefined(typeof(OnDeserializedAttribute)));
            if (methodInfo != null)
            {
                objectMapping.SetOnDeserializedMethod(GenerateCallbackDelegate<T>(methodInfo));
            }
            else if (type.GetInterfaces().Any(i => i == typeof(ISupportInitialize)))
            {
                objectMapping.SetOnDeserializedMethod(t => ((ISupportInitialize)t).EndInit());
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

        private void ProcessDefaultValue<T>(MemberInfo memberInfo, MemberMapping<T> memberMapping) where T : class
        {
            DefaultValueAttribute defaultValueAttribute = memberInfo.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttribute != null)
            {
                memberMapping.SetDefaultValue(defaultValueAttribute.Value);
            }

            if (memberInfo.IsDefined(typeof(JsonIgnoreIfDefaultAttribute)))
            {
                memberMapping.SetIngoreIfDefault(true);
            }
        }

        private void ProcessShouldSerializeMethod<T>(MemberMapping<T> memberMapping) where T : class
        {
            string shouldSerializeMethodName = "ShouldSerialize" + memberMapping.MemberInfo.Name;
            Type objectType = memberMapping.MemberInfo.DeclaringType;

            MethodInfo shouldSerializeMethodInfo = objectType.GetMethod(shouldSerializeMethodName, new Type[] { });
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
