using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;
using NGenerics.DataStructures.General;

namespace Hiro.Loaders
{
    /// <summary>
    /// Represents a class that can load a dependency map from a given set o fassemblies
    /// </summary>
    public class DependencyMapLoader
    {
        private IServiceLoader _serviceLoader;
        private IDefaultServiceResolver _defaultServiceResolver;

        /// <summary>
        /// Initializes a new instance of the DependencyMapLoader class.
        /// </summary>
        public DependencyMapLoader()
            : this(new ServiceLoader(), new DefaultServiceResolver())
        {
        }

        /// <summary>
        /// Initializes a new instance of the DependencyMapLoader class.
        /// </summary>
        /// <param name="serviceLoader">The service loader that will load services from a given assembly.</param>
        /// <param name="defaultServiceResolver">The resolver that will determine the default anonymous implementation for a particular service type.</param>
        public DependencyMapLoader(IServiceLoader serviceLoader, IDefaultServiceResolver defaultServiceResolver)
        {
            _serviceLoader = serviceLoader;
            _defaultServiceResolver = defaultServiceResolver;
        }

        /// <summary>
        /// Gets or sets the value indicating the predicate that will determine the services that will be loaded into the dependency map.
        /// </summary>
        /// <value>The predicate that will be used to determine which services will be loaded into the dependency map.</value>
        public Predicate<IServiceInfo> ServiceFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Loads a dependency map using the types in the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">The assembly that will be used to construct the dependency map.</param>
        /// <returns>A dependency map.</returns>
        public DependencyMap LoadFrom(Assembly assembly)
        {
            return LoadFrom(new Assembly[] { assembly });
        }

        /// <summary>
        /// Loads a dependency map using the types in the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">The list of assemblies that will be used to construct the dependency map.</param>
        /// <returns>A dependency map.</returns>
        public DependencyMap LoadFrom(IEnumerable<Assembly> assemblies)
        {
            var map = new DependencyMap();
            var serviceList = GetServiceList(assemblies);

            var defaultServices = GetDefaultServices(serviceList);
            var registeredServices = new List<IServiceInfo>();

            // Apply the service filter to the list of services
            // that will be added to the map
            Predicate<IServiceInfo> nullPredicate = info => true;
            Predicate<IServiceInfo> filter = ServiceFilter ?? nullPredicate;
            foreach (var service in defaultServices)
            {
                if (!filter(service))
                    continue;

                registeredServices.Add(service);
            }

            foreach (var list in serviceList.Values)
            {
                var filteredList = new List<IServiceInfo>();
                foreach (var service in list)
                {
                    if (!filter(service))
                        continue;

                    registeredServices.Add(service);
                }
            }

            map.Register(registeredServices);

            return map;
        }

        /// <summary>
        /// Loads a dependency map using the assemblies located in the target directory.
        /// </summary>
        /// <param name="directory">The directory that contains the assemblies that will be loaded into the dependency map.</param>
        /// <param name="filePattern">The search pattern that describes which assemblies will be loaded.</param>
        /// <returns>A dependency map.</returns>
        public DependencyMap LoadFrom(string directory, string filePattern)
        {
            return LoadFrom(directory, filePattern, new AssemblyLoader());
        }

        /// <summary>
        /// Loads a dependency map using the assemblies located in the target directory.
        /// </summary>
        /// <param name="directory">The directory that contains the assemblies that will be loaded into the dependency map.</param>
        /// <param name="filePattern">The search pattern that describes which assemblies will be loaded.</param>
        /// <param name="assemblyLoader">The assembly loader that will load assemblies into memory.</param>
        /// <returns>A dependency map.</returns>
        public DependencyMap LoadFrom(string directory, string filePattern, IAssemblyLoader assemblyLoader)
        {
            // Load the assemblies from the target directory
            var files = Directory.GetFiles(directory, filePattern);
            var assemblies = new List<Assembly>();
            foreach (var file in files)
            {
                var assembly = assemblyLoader.Load(file);
                assemblies.Add(assembly);
            }

            return LoadFrom(assemblies);
        }

        /// <summary>
        /// Gets the list of default services from a given service list.
        /// </summary>
        /// <param name="serviceList">The list of service implementations that will be used to determine the default service for each service type.</param>
        /// <returns></returns>
        private List<IServiceInfo> GetDefaultServices(HashList<Type, IServiceInfo> serviceList)
        {
            // Get the default services for each service type
            var defaultServices = new List<IServiceInfo>();
            foreach (var serviceType in serviceList.Keys)
            {
                var services = serviceList[serviceType];
                var defaultService = _defaultServiceResolver.GetDefaultService(serviceType, services);

                // Use the default service as the anonymous service
                var anonymousService = new ServiceInfo(defaultService.ServiceType, defaultService.ImplementingType, null);
                defaultServices.Add(anonymousService);
            }

            return defaultServices;
        }


        /// <summary>
        /// Obtains a list of services (grouped by type) from the list of assemblies.
        /// </summary>
        /// <param name="assemblies">The list of assemblies that contain the types that will be injected into the dependency map.</param>
        /// <returns>A list of services grouped by type.</returns>
        private HashList<Type, IServiceInfo> GetServiceList(IEnumerable<Assembly> assemblies)
        {
            var serviceList = new HashList<Type, IServiceInfo>();
            foreach (var assembly in assemblies)
            {
                var services = _serviceLoader.Load(assembly);
                foreach (var service in services)
                {
                    var serviceName = service.ServiceName;
                    var serviceType = service.ServiceType;

                    // Group the services by service type
                    serviceList.Add(serviceType, service);
                }
            }

            return serviceList;
        }
    }
}
