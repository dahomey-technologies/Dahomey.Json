using Dahomey.Json.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class DefaultObjectMappingConvention : IObjectMappingConvention
    {
        public void Apply<T>(JsonSerializerOptions options, ObjectMapping<T> objectMapping)
        {
            Type type = objectMapping.ObjectType;
            List<MemberMapping<T>> memberMappings = new List<MemberMapping<T>>();

            JsonDiscriminatorAttribute? discriminatorAttribute = type.GetCustomAttribute<JsonDiscriminatorAttribute>();

            if (discriminatorAttribute != null && options.GetDiscriminatorConventionRegistry().AnyConvention())
            {
                objectMapping.SetDiscriminator(discriminatorAttribute.Discriminator);
                objectMapping.SetDiscriminatorPolicy(discriminatorAttribute.Policy);
            }

            Type? namingPolicyType = type.GetCustomAttribute<JsonNamingPolicyAttribute>()?.NamingPolicyType;
            if (namingPolicyType != null)
            {
                JsonNamingPolicy? namingPolicy = (JsonNamingPolicy?)Activator.CreateInstance(namingPolicyType);

                if (namingPolicy == null)
                {
                    throw new JsonException($"Cannot instantiate naming policy {namingPolicyType}");
                }

                objectMapping.SetPropertyNamingPolicy(namingPolicy);
            }

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (PropertyInfo propertyInfo in properties)
            {
                JsonIgnoreAttribute? jsonIgnoreAttribute = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();
                if (jsonIgnoreAttribute != null
#if NET5_0
                    && jsonIgnoreAttribute.Condition == JsonIgnoreCondition.Always
#endif
                    )
                {
                        continue;
                }

                if (typeof(Delegate).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    continue;
                }

                if (propertyInfo.IsDefined(typeof(JsonExtensionDataAttribute)))
                {
                    objectMapping.MapExtensionData(propertyInfo);
                    continue;
                }

                MethodInfo? getMethod = propertyInfo.GetMethod;

                if (getMethod == null)
                {
                    continue;
                }

                if ((getMethod.IsPrivate || getMethod.IsStatic)
                    && !propertyInfo.IsDefined(typeof(JsonPropertyNameAttribute))
                    && !propertyInfo.IsDefined(typeof(JsonPropertyAttribute)))
                {
                    continue;
                }

                MemberMapping<T> memberMapping = new MemberMapping<T>(options, objectMapping, propertyInfo, propertyInfo.PropertyType);
                ProcessDefaultValue(propertyInfo, memberMapping, options);
                ProcessShouldSerializeMethod(memberMapping);
                ProcessRequired(propertyInfo, memberMapping);
                ProcessMemberName(propertyInfo, memberMapping);
                ProcessConverter(propertyInfo, memberMapping);
                ProcessDeserialize(propertyInfo, memberMapping);
                memberMappings.Add(memberMapping);
            }

            foreach (FieldInfo fieldInfo in fields)
            {
                JsonIgnoreAttribute? jsonIgnoreAttribute = fieldInfo.GetCustomAttribute<JsonIgnoreAttribute>();
                if (jsonIgnoreAttribute != null
#if NET5_0
                    && jsonIgnoreAttribute.Condition == JsonIgnoreCondition.Always
#endif
                    )
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

                if (typeof(Delegate).IsAssignableFrom(fieldType))
                {
                    continue;
                }

                MemberMapping<T> memberMapping = new MemberMapping<T>(options, objectMapping, fieldInfo, fieldInfo.FieldType);
                ProcessDefaultValue(fieldInfo, memberMapping, options);
                ProcessShouldSerializeMethod(memberMapping);
                ProcessRequired(fieldInfo, memberMapping);
                ProcessMemberName(fieldInfo, memberMapping);
                ProcessConverter(fieldInfo, memberMapping);

                memberMappings.Add(memberMapping);
            }

            objectMapping.AddMemberMappings(memberMappings);

            if (!type.IsAbstract)
            {
                ConstructorInfo[] constructorInfos = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                ConstructorInfo? constructorInfo = constructorInfos
                    .FirstOrDefault(c => c.IsDefined(typeof(JsonConstructorExAttribute))
#if NET5_0
                    || c.IsDefined(typeof(JsonConstructorAttribute))
#endif
                    );

                if (constructorInfo != null)
                {
                    CreatorMapping creatorMapping = objectMapping.MapCreator(constructorInfo);

                    JsonConstructorExAttribute? constructorAttribute = constructorInfo.GetCustomAttribute<JsonConstructorExAttribute>();
                    if (constructorAttribute != null && constructorAttribute.MemberNames != null)
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

        private void ProcessDefaultValue<T>(MemberInfo memberInfo, MemberMapping<T> memberMapping, JsonSerializerOptions options)
        {
            DefaultValueAttribute? defaultValueAttribute = memberInfo.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValueAttribute != null)
            {
                memberMapping.SetDefaultValue(defaultValueAttribute.Value);
            }

            if (memberInfo.IsDefined(typeof(JsonIgnoreIfDefaultAttribute)))
            {
                memberMapping.SetIngoreIfDefault(true);
            }

#if NET5_0
            if (options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault
                || options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull && memberMapping.MemberType.IsClass)
            {
                memberMapping.SetIngoreIfDefault(true);
            }

            JsonIgnoreAttribute? jsonIgnoreAttribute = memberInfo.GetCustomAttribute<JsonIgnoreAttribute>();
            if (jsonIgnoreAttribute != null)
            {
                if (jsonIgnoreAttribute.Condition == JsonIgnoreCondition.WhenWritingDefault
                    || jsonIgnoreAttribute.Condition == JsonIgnoreCondition.WhenWritingNull 
                    && (memberMapping.MemberType.IsClass || Nullable.GetUnderlyingType(memberMapping.MemberType) != null))
                {
                    memberMapping.SetIngoreIfDefault(true);
                }
            }
#endif
        }

        private void ProcessShouldSerializeMethod<T>(MemberMapping<T> memberMapping)
        {
            string shouldSerializeMethodName = "ShouldSerialize" + memberMapping.MemberInfo.Name;
            Type objectType = typeof(T);

            MethodInfo? shouldSerializeMethodInfo = objectType.GetMethod(
                shouldSerializeMethodName, 
                BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance, 
                null, new Type[] { }, null);

            if (shouldSerializeMethodInfo != null &&
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
            JsonRequiredAttribute? jsonRequiredAttribute = memberInfo.GetCustomAttribute<JsonRequiredAttribute>();
            if (jsonRequiredAttribute != null)
            {
                memberMapping.SetRequired(jsonRequiredAttribute.Policy);
            }
        }

        private void ProcessMemberName<T>(MemberInfo memberInfo, MemberMapping<T> memberMapping)
        {
            JsonPropertyNameAttribute? jsonPropertyNameAttribute = memberInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (jsonPropertyNameAttribute != null)
            {
                memberMapping.SetMemberName(jsonPropertyNameAttribute.Name);
            }
        }

        private void ProcessConverter<T>(MemberInfo memberInfo, MemberMapping<T> memberMapping)
        {
            JsonConverterAttribute? converterAttribute = memberInfo.GetCustomAttribute<JsonConverterAttribute>();
            if (converterAttribute != null)
            {
                Type? converterType = converterAttribute.ConverterType;

                if (converterType != null)
                {
                    JsonConverter? converter = (JsonConverter?)Activator.CreateInstance(converterType);

                    if (converter == null)
                    {
                        throw new JsonException($"Cannot instantiate {converterType}");
                    }

                    memberMapping.SetConverter(converter);
                }
            }
        }

        private void ProcessDeserialize<T>(MemberInfo memberInfo, MemberMapping<T> memberMapping)
        {
            if (memberInfo.IsDefined(typeof(JsonDeserializeAttribute)))
            {
                memberMapping.ForceDeserialize();
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
