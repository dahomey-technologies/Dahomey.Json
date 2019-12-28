using Dahomey.Json.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Dahomey.Json.Serialization.Converters.Mappings
{
    public class ObjectMappingConventionRegistry
    {
        private readonly ConcurrentDictionary<Type, IObjectMappingConvention> _conventions = new ConcurrentDictionary<Type, IObjectMappingConvention>();
        private readonly ConcurrentStack<IObjectMappingConventionProvider> _providers = new ConcurrentStack<IObjectMappingConventionProvider>();

        public ObjectMappingConventionRegistry()
        {
            // order matters. It's in reverse order of how they'll get consumed
            RegisterProvider(new DefaultObjectMappingConventionProvider());
            RegisterProvider(new AnonymousObjectMappingConventionProvider());
        }

        /// <summary>
        /// Gets the object mapping convention for the specified <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// The object mapping convention.
        /// </returns>
        public IObjectMappingConvention Lookup<T>()
        {
            return Lookup(typeof(T));
        }

        /// <summary>
        /// Gets the object mapping convention for the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The convention.
        /// </returns>
        public IObjectMappingConvention Lookup(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (type.IsGenericType && type.ContainsGenericParameters)
            {
                throw new ArgumentException(
                    $"Generic type {type.FullName} has unassigned type parameters.",
                    "type");
            }

            return _conventions.GetOrAdd(type, CreateConvention);
        }

        /// <summary>
        /// Registers the convention
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="convention">The convention.</param>
        public bool RegisterConvention(Type type, IObjectMappingConvention convention)
        {
            return _conventions.TryAdd(type, convention);
        }

        /// <summary>
        /// Registers the object mapping convention provider. This behaves like a stack, so the 
        /// last convention registered is the first convention consulted.
        /// </summary>
        /// <param name="provider">The object mapping convention provider.</param>
        public void RegisterProvider(IObjectMappingConventionProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            _providers.Push(provider);
        }

        private IObjectMappingConvention CreateConvention(Type type)
        {
            IObjectMappingConvention convention = _providers
                .Select(provider => provider.GetConvention(type))
                .FirstOrDefault(provider => provider != null);

            if (convention == null)
            {
                throw new JsonException($"No convention found for type {type.FullName}.");
            }

            return convention;
        }
    }
}
