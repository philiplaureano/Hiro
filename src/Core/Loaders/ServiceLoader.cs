using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;
using NGenerics.DataStructures.General;

namespace Hiro.Loaders
{
    /// <summary>
    /// Represents a type that can load services into memory.
    /// </summary>
    public class ServiceLoader : IServiceLoader
    {
        /// <summary>
        /// Initializes a new instance of the ServiceLoader class.
        /// </summary>
        public ServiceLoader()
            : this(new TypeLoader(), new TypeFilter())
        {
        }

        /// <summary>
        /// Initializes a new instance of the ServiceLoader class.
        /// </summary>
        /// <param name="typeLoader">The type loader that will load types into memory.</param>
        /// <param name="typeFilter">The filter that will be used to determine which types should be loaded.</param>
        public ServiceLoader(ITypeLoader typeLoader, ITypeFilter typeFilter)
        {
            TypeLoader = typeLoader;
            TypeFilter = typeFilter;
        }

        /// <summary>
        /// Gets or sets a value indicating the <see cref="ITypeLoader"/> instance that will be used to load a type into memory.
        /// </summary>
        /// <value>The type loader.</value>
        public ITypeLoader TypeLoader
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating hte <see cref="ITypeFilter"/> instance that will be used to determine which types should be loaded into memory.
        /// </summary>
        public ITypeFilter TypeFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Loads services from the given assembly.
        /// </summary>
        /// <param name="targetAssembly">The assembly that contains the types to be loaded.</param>
        /// <returns>The list of services.</returns>
        public IEnumerable<IServiceInfo> Load(Assembly targetAssembly)
        {
            if (TypeLoader == null || TypeFilter == null)
                yield break;

            IEnumerable<Type> loadedTypes = TypeLoader.LoadTypes(targetAssembly);

            // Load all public types that can be instantiated
            Predicate<Type> concreteTypeFilter = t => t.IsPublic && !t.IsAbstract && !t.IsInterface && !t.IsValueType && !t.IsGenericTypeDefinition;
            var types = TypeFilter.GetTypes(loadedTypes, concreteTypeFilter);

            foreach (var type in types)
            {
                // Determine the interfaces for the current type
                var interfaces = type.GetInterfaces();
                foreach (var interfaceType in interfaces)
                {
                    var serviceName = type.Name;

                    yield return new ServiceInfo(interfaceType, type, serviceName);
                }
            }
        }       
    }
}
