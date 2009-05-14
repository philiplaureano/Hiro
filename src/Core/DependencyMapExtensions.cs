using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro;
using Hiro.Containers;
using Hiro.Implementations;
using Hiro.Interfaces;
using LinFu.Reflection.Emit;

namespace Hiro
{
    /// <summary>
    /// A helper class that adds extension methods to the <see cref="DependencyMap"/> class.
    /// </summary>
    public static class DependencyMapExtensions
    {
        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will implement the service type.</typeparam>
        /// <param name="map">The dependency map.</param>
        public static void AddService<TService, TImplementation>(this IDependencyMap map)
            where TImplementation : TService
        {
            map.AddService(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will implement the service type.</typeparam>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceName">The service name.</param>
        public static void AddService<TService, TImplementation>(this IDependencyMap map, string serviceName)
            where TImplementation : TService
        {
            map.AddService(serviceName, typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public static void AddService(this IDependencyMap map, string serviceName, Type serviceType, Type implementingType)
        {
            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            map.AddService(new Dependency(serviceType, serviceName), new TransientType(implementingType, map));
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public static void AddService(this IDependencyMap map, Type serviceType, Type implementingType)
        {
            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            map.AddService(new Dependency(serviceType), new TransientType(implementingType, map));
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will implement the service type.</typeparam>
        /// <param name="map">The dependency map.</param>
        public static void AddSingletonService<TService, TImplementation>(this IDependencyMap map)
            where TImplementation : TService
        {
            map.AddService(new Dependency(typeof(TService)), new SingletonType(typeof(TImplementation), map));
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will implement the service type.</typeparam>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceName">The service name.</param>
        public static void AddSingletonService<TService, TImplementation>(this IDependencyMap map, string serviceName)
        {
            map.AddService(new Dependency(typeof(TService), serviceName), new SingletonType(typeof(TImplementation), map));
        }

        /// <summary>
        /// Adds a singleton service to the dependency map.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public static void AddSingletonService(this IDependencyMap map, Type serviceType, Type implementingType)
        {
            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            map.AddService(new Dependency(serviceType), new SingletonType(implementingType, map));
        }

        /// <summary>
        /// Adds a singleton service to the dependency map.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public static void AddSingletonService(this IDependencyMap map, string serviceName, Type serviceType, Type implementingType)
        {
            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            map.AddService(new Dependency(serviceType, serviceName), new SingletonType(implementingType, map));
        }

        /// <summary>
        /// Determines whether or not a service exists within the dependency map.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceName">The service name.</param>
        /// <returns><c>true</c> if the service exists; otherwise, it will return <c>false</c>.</returns>
        public static bool Contains(this IDependencyMap map, Type serviceType, string serviceName)
        {
            return map.Contains(new Dependency(serviceType, serviceName));
        }

        /// <summary>
        /// Determines whether or not a service exists within the dependency map.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceType">The service type.</param>
        /// <returns><c>true</c> if the service exists; otherwise, it will return <c>false</c>.</returns>
        public static bool Contains(this IDependencyMap map, Type serviceType)
        {
            return map.Contains(new Dependency(serviceType));
        }

        /// <summary>
        /// Compiles and instantiates a container instance using the current dependencies in the dependency map.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <returns>A <see cref="IMicroContainer"/> instance.</returns>
        public static IMicroContainer CreateContainer(this IDependencyMap map)
        {
            var compiler = new ContainerCompiler();
            var assembly = compiler.Compile(map);
            var loadedAssembly = assembly.ToAssembly();

            var containerType = (from t in loadedAssembly.GetTypes()
                                where typeof(IMicroContainer).IsAssignableFrom(t)
                                select t).First();

            IMicroContainer result = (IMicroContainer)Activator.CreateInstance(containerType);

            return result;
        }
    }
}
