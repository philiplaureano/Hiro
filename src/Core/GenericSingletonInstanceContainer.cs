using System;
using Hiro.Interfaces;

namespace Hiro
{
    /// <summary>
    /// Represents a container type that instantiates generic singleton types at runtime.
    /// </summary>
    public class GenericSingletonInstanceContainer : GenericInstanceContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSingletonInstanceContainer"/> class.
        /// </summary>
        /// <param name="serviceName">The name of the generic service.</param>
        /// <param name="genericServiceType">The generic type that represents the service type.</param>
        /// <param name="genericTypeImplementation">The type that represents the generic service type implementation.</param>
        /// <param name="dependencyContainer">The dependency map that describes the list of services that will be available to the instantiated generic types.</param>
        public GenericSingletonInstanceContainer(string serviceName, Type genericServiceType, Type genericTypeImplementation, IDependencyContainer dependencyContainer) : base(serviceName, genericServiceType, genericTypeImplementation, dependencyContainer)
        {
        }

        /// <summary>
        /// Registers the generic service type as a singleton type.
        /// </summary>
        /// <param name="serviceType">The service type that will be registered.</param>
        /// <param name="concreteType">The generic concrete type that will implement the generic service type.</param>
        /// <param name="map">The dependency map that contains all the dependencies.</param>
        protected override void Register(Type serviceType, Type concreteType, DependencyMap map)
        {
            map.AddSingletonService(serviceType, concreteType);
        }
    }
}