using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;
using Hiro.Functors.Core;
using Hiro.Interfaces;

namespace Hiro
{
    /// <summary>
    /// Represents a class that adds functor factory support to any given <see cref="IDependencyMap"/> instance.
    /// </summary>
    public static class FunctorExtensions
    {
        /// <summary>
        /// Adds a service implementation to the dependency map that uses the given <paramref name="factoryFunctor"/>
        /// to instantiate the service itself.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="map">The dependency map.</param>
        /// <param name="factoryFunctor">The factory functor that will be used to instantiate the service type.</param>
        public static void AddService<T>(this IDependencyMap map,
            Func<IMicroContainer, T> factoryFunctor)
        {
            Func<IMicroContainer, object> adapter = container => factoryFunctor(container);
            map.AddService(null, typeof(T), adapter);
        }

        /// <summary>
        /// Adds a service implementation to the dependency map that uses the given <paramref name="factoryFunctor"/>
        /// to instantiate the service itself.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="factoryFunctor">The factory functor that will be used to instantiate the service type.</param>
        public static void AddService<T>(this IDependencyMap map, string serviceName,
            Func<IMicroContainer, T> factoryFunctor)
        {
            Func<IMicroContainer, object> adapter = container => factoryFunctor(container);
            map.AddService(serviceName, typeof(T), adapter);
        }

        /// <summary>
        /// Adds a service implementation to the dependency map that uses the given <paramref name="factoryFunctor"/>
        /// to instantiate the service itself.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="factoryFunctor">The factory functor that will be used to instantiate the service type.</param>
        public static void AddService(this IDependencyMap map, string serviceName, System.Type serviceType,
            Func<IMicroContainer, object> factoryFunctor)
        {
            var dependency = new Dependency(serviceType, serviceName);
            var implementation = new FunctorCall(serviceType, factoryFunctor);

            map.AddService(dependency, implementation);
        }

        /// <summary>
        /// Adds a service implementation to the dependency map that uses the given <paramref name="factoryFunctor"/>
        /// to instantiate the service itself.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="factoryFunctor">The factory functor that will be used to instantiate the service type.</param>
        public static void AddService(this IDependencyMap map, System.Type serviceType,
            Func<IMicroContainer, object> factoryFunctor)
        {
            map.AddService(new Dependency(serviceType), factoryFunctor);
        }

        /// <summary>
        /// Adds a service implementation to the dependency map that uses the given <paramref name="factoryFunctor"/>
        /// to instantiate the service itself.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="dependency">The <see cref="IDependency"/> instance that describes the dependency that will be registered with the dependency map itself. <see</param>
        /// <param name="factoryFunctor">The factory functor that will be used to instantiate the service type.</param>
        public static void AddService(this IDependencyMap map, IDependency dependency, Func<IMicroContainer, object> factoryFunctor)
        {
            var implementation = new FunctorCall(dependency.ServiceType, factoryFunctor);
            map.AddService(dependency, implementation);
        }
    }
}
