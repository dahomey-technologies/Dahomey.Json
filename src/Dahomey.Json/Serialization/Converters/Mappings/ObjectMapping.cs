using Dahomey.Json.Serialization.Conventions;
using Dahomey.Json.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    /// <summary>
    /// Represent a mapping between a class and Cbor serialization framework
    /// </summary>
    /// <typeparam name="T">The class.</typeparam>
    public class ObjectMapping<T> : IObjectMapping
    {
        private readonly JsonSerializerOptions _options;
        private List<IMemberMapping> _memberMappings = new List<IMemberMapping>();
        private ICreatorMapping? _creatorMapping = null;
        private Action? _orderByAction = null; 

        public Type ObjectType { get; private set; }

        public JsonNamingPolicy? PropertyNamingPolicy { get; private set; }
        public IReadOnlyCollection<IMemberMapping> MemberMappings => _memberMappings;
        public ICreatorMapping? CreatorMapping => _creatorMapping;
        public Delegate? OnSerializingMethod { get; private set; }
        public Delegate? OnSerializedMethod { get; private set; }
        public Delegate? OnDeserializingMethod { get; private set; }
        public Delegate? OnDeserializedMethod { get; private set; }
        public DiscriminatorPolicy DiscriminatorPolicy { get; private set; }
        public object? Discriminator { get; private set; }
        public IExtensionDataMemberConverter? ExtensionData { get; private set; }

        public ObjectMapping(JsonSerializerOptions options)
        {
            _options = options;
            ObjectType = typeof(T);
        }

        void IObjectMapping.AutoMap()
        {
            AutoMap();
        }

        public ObjectMapping<T> AutoMap()
        {
            IObjectMappingConvention convention = _options.GetObjectMappingConventionRegistry().Lookup<T>();
            convention.Apply(_options, this);
            return this;
        }

        public ObjectMapping<T> SetDiscriminator(object discriminator)
        {
            Discriminator = discriminator;
            return this;
        }

        public ObjectMapping<T> SetPropertyNamingPolicy(JsonNamingPolicy propertyNamingPolicy)
        {
            PropertyNamingPolicy = propertyNamingPolicy;
            return this;
        }

        /// <summary>
        /// Creates a member map and adds it to the object mapping.
        /// </summary>
        /// <typeparam name="TM">The member type.</typeparam>
        /// <param name="memberLambda">A lambda expression specifying the member.</param>
        /// <returns>The member map.</returns>
        public MemberMapping<T> MapMember<TM>(Expression<Func<T, TM>> memberLambda)
        {
            (MemberInfo memberInfo, Type memberType) = GetMemberInfoFromLambda(memberLambda);
            return MapMember(memberInfo, memberType);
        }

        public MemberMapping<T> MapMember(MemberInfo memberInfo, Type memberType)
        {
            MemberMapping<T> memberMapping = new MemberMapping<T>(_options, this, memberInfo, memberType);
            _memberMappings.Add(memberMapping);
            return memberMapping;
        }

        public MemberMapping<T> MapMember(FieldInfo fieldInfo)
        {
            return MapMember(fieldInfo, fieldInfo.FieldType);
        }

        public MemberMapping<T> MapMember(PropertyInfo propertyInfo)
        {
            return MapMember(propertyInfo, propertyInfo.PropertyType);
        }

        public ObjectMapping<T> MapExtensionData<TM>(Expression<Func<T, TM>> memberLambda)
        {
            (MemberInfo memberInfo, Type memberType) = GetMemberInfoFromLambda(memberLambda);
            return MapExtensionData((PropertyInfo)memberInfo);
        }

        public ObjectMapping<T> MapExtensionData(PropertyInfo propertyInfo)
        {
            Type propertyType = propertyInfo.PropertyType;
            if (!typeof(IDictionary<string, JsonElement>).IsAssignableFrom(propertyType) &&
                !typeof(IDictionary<string, object>).IsAssignableFrom(propertyType))
            { 
                throw new JsonException("Invalid Serialization DataExtension Property");
            }

            ExtensionData = (IExtensionDataMemberConverter?)
                Activator.CreateInstance(typeof(ExtensionDataMemberConverter<,,>)
                    .MakeGenericType(typeof(T), propertyType, propertyType.GetGenericArguments()[1]),
                    propertyInfo, _options);

            if (ExtensionData == null)
            {
                throw new JsonException($"Cannot instantiate ExtensionDataMemberConverter for type {typeof(T)}");
            }

            return this;
        }

        public ObjectMapping<T> AddMemberMappings(IReadOnlyCollection<IMemberMapping> memberMappings)
        {
            _memberMappings.AddRange(memberMappings);
            return this;
        }

        public ObjectMapping<T> SetMemberMappings(IEnumerable<IMemberMapping> memberMappings)
        {
            _memberMappings = memberMappings.ToList();
            return this;
        }

        public ObjectMapping<T> ClearMemberMappings()
        {
            _memberMappings.Clear();
            return this;
        }

        public CreatorMapping MapCreator(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                throw new ArgumentNullException("constructorInfo");
            }

            CreatorMapping creatorMapping = new CreatorMapping(this, constructorInfo);
            _creatorMapping = creatorMapping;
            return creatorMapping;
        }


        private CreatorMapping MapCreator(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            CreatorMapping creatorMapping = new CreatorMapping(this, method);
            _creatorMapping = creatorMapping;
            return creatorMapping;
        }

        public CreatorMapping MapCreator(Delegate creatorFunc)
        {
            if (creatorFunc == null)
            {
                throw new ArgumentNullException("creatorFunc");
            }

            CreatorMapping creatorMapping = new CreatorMapping(this, creatorFunc);
            _creatorMapping = creatorMapping;
            return creatorMapping;
        }

        public CreatorMapping MapCreator(Expression<Func<T, T>> creatorLambda)
        {
            if (creatorLambda == null)
            {
                throw new ArgumentNullException("creatorLambda");
            }

            if (creatorLambda.Body is NewExpression newExpression && newExpression.Constructor != null)
            {
                return MapCreator(newExpression.Constructor);
            }
            else if (creatorLambda.Body is MethodCallExpression methodCallExpression && methodCallExpression.Object == null)
            {
                return MapCreator(methodCallExpression.Method);
            }

            throw new ArgumentException("creatorLambda should be a 'new' or a static 'method call' expression");
        }

        public ObjectMapping<T> SetCreatorMapping(ICreatorMapping creatorMapping)
        {
            _creatorMapping = creatorMapping;
            return this;
        }

        public ObjectMapping<T> SetOnSerializingMethod(Action<T> onSerializingMethod)
        {
            OnSerializingMethod = onSerializingMethod;
            return this;
        }

        public ObjectMapping<T> SetOnSerializedMethod(Action<T> onSerializedMethod)
        {
            OnSerializedMethod = onSerializedMethod;
            return this;
        }

        public ObjectMapping<T> SetOnDeserializingMethod(Action<T> onDeserializingMethod)
        {
            OnDeserializingMethod = onDeserializingMethod;
            return this;
        }

        public ObjectMapping<T> SetOnDeserializedMethod(Action<T> onDeserializedMethod)
        {
            OnDeserializedMethod = onDeserializedMethod;
            return this;
        }

        public ObjectMapping<T> SetDiscriminatorPolicy(DiscriminatorPolicy discriminatorPolicy)
        {
            DiscriminatorPolicy = discriminatorPolicy;
            return this;
        }

        public void SetOrderBy<TP>(Func<IMemberMapping, TP> propertySelector)
        {
            _orderByAction = () =>
            {
                _memberMappings = _memberMappings
                    .OrderBy(propertySelector)
                    .ToList();
            };
        }

        public bool IsCreatorMember(ReadOnlySpan<byte> memberName)
        {
            if (CreatorMapping == null || CreatorMapping.MemberNames == null)
            {
                return false;
            }

            foreach (ReadOnlyMemory<byte> creatorMemberName in CreatorMapping.MemberNames)
            {
                if (creatorMemberName.Span.SequenceEqual(memberName))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddDiscriminatorMapping()
        {
            DiscriminatorMapping<T> memberMapping = new DiscriminatorMapping<T>(_options.GetDiscriminatorConventionRegistry(), this);
            _memberMappings.Insert(0, memberMapping);
        }

        public void Initialize()
        {
            foreach(IMemberMapping mapping in _memberMappings)
            {
                if (mapping is IMappingInitialization memberInitialization)
                {
                    memberInitialization.Initialize();
                }
            }

            if (CreatorMapping != null && CreatorMapping is IMappingInitialization creatorInitialization)
            {
                creatorInitialization.Initialize();
            }
        }

        public void PostInitialize()
        {
            foreach (IMemberMapping mapping in _memberMappings)
            {
                if (mapping is IMappingInitialization memberInitialization)
                {
                    memberInitialization.PostInitialize();
                }
            }

            if (CreatorMapping != null && CreatorMapping is IMappingInitialization creatorInitialization)
            {
                creatorInitialization.PostInitialize();
            }

            _orderByAction?.Invoke();
        }

        private static (MemberInfo, Type) GetMemberInfoFromLambda<TM>(Expression<Func<T, TM>> memberLambda)
        {
            var body = memberLambda.Body;
            MemberExpression memberExpression;
            switch (body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    memberExpression = (MemberExpression)body;
                    break;
                case ExpressionType.Convert:
                    var convertExpression = (UnaryExpression)body;
                    memberExpression = (MemberExpression)convertExpression.Operand;
                    break;
                default:
                    throw new InvalidOperationException("Invalid lambda expression");
            }

            MemberInfo memberInfo = memberExpression.Member;

            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    return (memberInfo, propertyInfo.PropertyType);

                case FieldInfo fieldfInfo:
                    return (memberInfo, fieldfInfo.FieldType);

                default:
                    throw new InvalidOperationException("Invalid lambda expression");
            }
        }
    }
}
