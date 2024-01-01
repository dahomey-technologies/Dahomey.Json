using Dahomey.Json.Attributes;
using Dahomey.Json.Serialization.Converters.Mappings;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Conventions
{
    public class DiscriminatorConventionRegistry
    {
        private readonly JsonSerializerOptions _options;
        private readonly ConcurrentStack<IDiscriminatorConvention> _conventions = new ConcurrentStack<IDiscriminatorConvention>();
        private readonly ConcurrentDictionary<Type, IDiscriminatorConvention?> _conventionsByType = new ConcurrentDictionary<Type, IDiscriminatorConvention?>();

        public DiscriminatorPolicy DiscriminatorPolicy { get; set; }

        public DiscriminatorConventionRegistry(JsonSerializerOptions options)
        {
            _options = options;
            // order matters. It's in reverse order of how they'll get consumed
            RegisterConvention(new DefaultDiscriminatorConvention<string>(options));
        }

        public bool AnyConvention()
        {
            return _conventions.Count != 0;
        }

        /// <summary>
        /// Registers the convention.This behaves like a stack, so the 
        /// last convention registered is the first convention consulted.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="convention">The convention.</param>
        public void RegisterConvention(IDiscriminatorConvention convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            _conventions.Push(convention);
        }

        public void ClearConventions()
        {
            _conventions.Clear();
        }

        public IDiscriminatorConvention? GetConvention(Type type)
        {
            if(_conventionsByType.TryGetValue(type, out IDiscriminatorConvention? convention))
            {
                return convention;
            }

            convention = InternalGetConvention(type);
            if(convention != null)
            {
                _conventionsByType.TryAdd(type, convention);
            }
            return convention;
        }

        public void RegisterAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                RegisterType(type);
            }
        }

        public void RegisterType(Type type)
        {
            // First call will force the registration.
            GetConvention(type);
        }

        public void RegisterType<T>()
        {
            RegisterType(typeof(T));
        }

        private IDiscriminatorConvention? InternalGetConvention(Type type)
        {
            IDiscriminatorConvention? convention = _conventions.FirstOrDefault(c => c.TryRegisterType(type));

            if (convention != null)
            {
                IObjectMapping objectMapping = _options.GetObjectMappingRegistry().Lookup(type);
                objectMapping.AddDiscriminatorMapping();

                // setup discriminator for all base types
                for (Type? currentType = type.BaseType; currentType != null && currentType != typeof(object); currentType = currentType.BaseType)
                {
                    if (_conventionsByType.TryAdd(currentType, convention))
                    {
                        objectMapping = _options.GetObjectMappingRegistry().Lookup(currentType);
                        objectMapping.AddDiscriminatorMapping();
                    }
                }

                // setup discriminator for all interfaces
                foreach (Type @interface in type.GetInterfaces())
                {
                    _conventionsByType.TryAdd(@interface, convention);
                }
            }

            return convention;
        }
    }
}
