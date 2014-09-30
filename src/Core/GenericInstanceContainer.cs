using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hiro.Containers;
using Hiro.Interfaces;

namespace Hiro
{
    /// <summary>
    /// Represents a container type that instantiates generic types at runtime.
    /// </summary>
    public class GenericInstanceContainer : IMicroContainer
    {
        private readonly string _serviceName;
        private readonly Type _genericServiceType;
        private readonly Type _genericTypeImplementation;
        private readonly IDependencyContainer _dependencyContainer;
        private readonly IDictionary<Type, IMicroContainer> _containerMap = new ConcurrentDictionary<Type, IMicroContainer>();
        private IDictionary<IDependency, IImplementation> _serviceMap;
        private IMicroContainer _baseContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericInstanceContainer"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the generic service.</param>
        /// <param name="genericServiceType">The generic type that represents the service type.</param>
        /// <param name="genericTypeImplementation">The type that represents the generic service type implementation.</param>
        /// <param name="dependencyContainer">The dependency map that describes the list of services that will be available to the instantiated generic types.</param>
        public GenericInstanceContainer(string serviceName, Type genericServiceType, Type genericTypeImplementation, IDependencyContainer dependencyContainer)
        {
            if (!genericServiceType.IsGenericTypeDefinition)
                throw new ArgumentException("The type must be a generic type definition", "genericServiceType");

            if (!genericTypeImplementation.IsGenericTypeDefinition)
                throw new ArgumentException("The type must be a generic type definition", "genericTypeImplementation");

            _serviceName = serviceName;
            _genericServiceType = genericServiceType;
            _genericTypeImplementation = genericTypeImplementation;
            _dependencyContainer = dependencyContainer;
        }

        /// <summary>
        /// Determines whether or not the container can instantiate the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="key">The name of the service</param>
        /// <returns>Returns <c>true</c> if the type can be instantiated by the container.</returns>
        public bool Contains(Type serviceType, string key)
        {
            if (_serviceName != null && _serviceName != key)
                return false;

            if (!serviceType.IsGenericType && !serviceType.IsGenericTypeDefinition)
                return false;

            // Match the arity of the generic parameters
            var arguments = serviceType.GetGenericArguments();
            var expectedArgumentCount = _genericServiceType.GetGenericArguments().Length;

            if (arguments.Length != expectedArgumentCount)
                return false;

            var projectedServiceType = _genericServiceType.MakeGenericType(arguments);


            var isCompatible = projectedServiceType.IsAssignableFrom(serviceType);

            return isCompatible;
        }

        /// <summary>
        /// Returns all generic instances that match the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns>A list of objects that represent the <paramref name="serviceType"/>.</returns>
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            yield return GetInstance(serviceType, null);
        }

        /// <summary>
        /// Attempts to get an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="key">The service name.</param>
        /// <returns>An object instance that matches the given service type. This method will return null if that service isn't available.</returns>
        public object GetInstance(Type serviceType, string key)
        {
            if (!Contains(serviceType, key))
                return null;

            // Compile the container that will be used
            // to satisfy the dependencies for the service type
            if (_baseContainer == null)
                BuildBaseContainer();

            // Use the existing container, if possible
            if (!_containerMap.ContainsKey(serviceType))
            {
                // Create a new container that constructs the service type
                // and redirects all other calls to the base container
                var map = new DependencyMap();
                var availableDependencies = _serviceMap.Keys;

                foreach (var dependency in availableDependencies)
                {
                    // The base container will handle all the other dependencies
                    map.AddDeferredService(dependency.ServiceName, dependency.ServiceType);
                }

                // Add the service type itself
                var arguments = serviceType.GetGenericArguments();
                var concreteType = _genericTypeImplementation.MakeGenericType(arguments);
                Register(serviceType, concreteType, map);

                // Compile the container
                var container = map.CreateContainer();

                // Defer the other calls to the base container
                container.NextContainer = _baseContainer;

                _containerMap[serviceType] = container;
            }

            var result = _containerMap[serviceType].GetInstance(serviceType, key);
            if (result == null && NextContainer != this && NextContainer != null)
                return NextContainer.GetInstance(serviceType, key);

            return result;
        }

        /// <summary>
        /// Registers the generic service type.
        /// </summary>
        /// <param name="serviceType">The service type that will be registered.</param>
        /// <param name="concreteType">The generic concrete type that will implement the generic service type.</param>
        /// <param name="map">The dependency map that contains all the dependencies.</param>
        protected virtual void Register(Type serviceType, Type concreteType, DependencyMap map)
        {
            map.AddService(serviceType, concreteType);
        }

        /// <summary>
        /// Gets or sets the value indicating the next container in the <see cref="IMicroContainer"/> chain.
        /// </summary>
        public IMicroContainer NextContainer { get; set; }

        private void BuildBaseContainer()
        {
            _serviceMap = _dependencyContainer.GetServiceMap();

            var map = new DependencyMap();
            foreach (var dependency in _serviceMap.Keys)
            {
                map.AddService(dependency, _serviceMap[dependency]);
            }

            _baseContainer = map.CreateContainer();
        }
    }
}