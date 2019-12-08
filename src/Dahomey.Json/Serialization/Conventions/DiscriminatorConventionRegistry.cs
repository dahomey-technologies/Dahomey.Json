using Dahomey.Json.Util;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Conventions
{
    public class DiscriminatorConventionRegistry : AbstractRegistry<DiscriminatorConventionRegistry>
    {
        private readonly ConcurrentStack<IDiscriminatorConvention> _conventions = new ConcurrentStack<IDiscriminatorConvention>();
        private readonly ConcurrentDictionary<Type, IDiscriminatorConvention> _conventionsByType = new ConcurrentDictionary<Type, IDiscriminatorConvention>();

        public DiscriminatorPolicy DiscriminatorPolicy { get; set; }

        public DiscriminatorConventionRegistry(JsonSerializerOptions options)
        {
            // order matters. It's in reverse order of how they'll get consumed
            RegisterConvention(new DefaultDiscriminatorConvention(options));
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

        public IDiscriminatorConvention GetConvention(Type type)
        {
            return _conventionsByType.GetOrAdd(type, t => InternalGetConvention(t));
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

        private IDiscriminatorConvention InternalGetConvention(Type type)
        {
            IDiscriminatorConvention convention = _conventions.FirstOrDefault(c => c.TryRegisterType(type));

            if (convention != null)
            {
                // setup discriminator for all base types
                for (Type currentType = type.BaseType; currentType != null && currentType != typeof(object); currentType = currentType.BaseType)
                {
                    _conventionsByType.TryAdd(currentType, convention);
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
