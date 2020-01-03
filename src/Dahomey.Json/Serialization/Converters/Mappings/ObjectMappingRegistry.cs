using System;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class ObjectMappingRegistry
    {
        private readonly ConcurrentDictionary<Type, IObjectMapping> _objectMappings
            = new ConcurrentDictionary<Type, IObjectMapping>();
        private readonly JsonSerializerOptions _options;

        public ObjectMappingRegistry(JsonSerializerOptions options)
        {
            _options = options;
        }

        public void Register(IObjectMapping objectMapping)
        {
            IMappingInitialization mappingInitialization = objectMapping as IMappingInitialization;
            if (mappingInitialization != null)
            {
                mappingInitialization.Initialize();
            }

            _objectMappings.AddOrUpdate(objectMapping.ObjectType, objectMapping,
                (type, existingObjectMapping) => objectMapping);

            if (objectMapping.Discriminator == null)
            {
                _options.GetDiscriminatorConventionRegistry().RegisterType(objectMapping.ObjectType);
            }

            if (mappingInitialization != null)
            {
                mappingInitialization.PostInitialize();
            }
        }

        public void Register<T>()
        {
            ObjectMapping<T> objectMapping = new ObjectMapping<T>(_options);
            Register(objectMapping);
        }

        public void Register<T>(Action<ObjectMapping<T>> initializer)
        {
            ObjectMapping<T> objectMapping = new ObjectMapping<T>(_options);
            initializer(objectMapping);
            Register(objectMapping);
        }

        public IObjectMapping Lookup<T>()
        {
            return Lookup(typeof(T));
        }

        public IObjectMapping Lookup(Type type)
        {
            return _objectMappings.GetOrAdd(type, t => CreateDefaultObjectMapping(type));
        }

        private IObjectMapping CreateDefaultObjectMapping(Type type)
        {
            IObjectMapping objectMapping =
                (IObjectMapping)Activator.CreateInstance(typeof(ObjectMapping<>).MakeGenericType(type), _options);

            objectMapping.AutoMap();

            if (objectMapping is IMappingInitialization mappingInitialization)
            {
                mappingInitialization.Initialize();
            }

            return objectMapping;
        }
    }
}
