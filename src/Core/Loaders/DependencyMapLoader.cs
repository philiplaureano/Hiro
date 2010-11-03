using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Hiro.Containers;
using Hiro.Implementations;
using Hiro.Interfaces;
using Hiro.Resolvers;
using NGenerics.DataStructures.General;

namespace Hiro.Loaders
{
    /// <summary>
    /// Represents a class that can load a dependency map from a given set o fassemblies
    /// </summary>
    public class DependencyMapLoader
    {
        private readonly IConstructorResolver _constructorResolver;
        private readonly ITypeLoader _typeLoader;
        private readonly IServiceLoader _serviceLoader;
        private readonly IDefaultServiceResolver _defaultServiceResolver;

        /// <summary>
        /// Initializes a new instance of the DependencyMapLoader class.
        /// </summary>
        public DependencyMapLoader()
            : this(new TypeLoader(), new ServiceLoader(), new DefaultServiceResolver())
        {
        }

        /// <summary>
        /// Initializes a new instance of the DependencyMapLoader class.
        /// </summary>
        public DependencyMapLoader(IConstructorResolver constructorResolver)
            : this(constructorResolver, new TypeLoader(), new ServiceLoader(), new DefaultServiceResolver())
        {
        }

        /// <summary>
        /// Initializes a new instance of the DependencyMapLoader class.
        /// </summary>
        /// <param name="typeLoader">The type loader that will load the service types from each assembly.</param>
        /// <param name="serviceLoader">The service loader that will load services from a given assembly.</param>
        /// <param name="defaultServiceResolver">The resolver that will determine the default anonymous implementation for a particular service type.</param>
        public DependencyMapLoader(ITypeLoader typeLoader, IServiceLoader serviceLoader, IDefaultServiceResolver defaultServiceResolver)
            : this(new ConstructorResolver(), typeLoader, serviceLoader, defaultServiceResolver)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DependencyMapLoader class.
        /// </summary>
        /// <param name="constructorResolver"></param>
        /// <param name="typeLoader">The type loader that will load the service types from each assembly.</param>
        /// <param name="serviceLoader">The service loader that will load services from a given assembly.</param>
        /// <param name="defaultServiceResolver">The resolver that will determine the default anonymous implementation for a particular service type.</param>
        public DependencyMapLoader(IConstructorResolver constructorResolver, ITypeLoader typeLoader, IServiceLoader serviceLoader, IDefaultServiceResolver defaultServiceResolver)
        {
            _constructorResolver = constructorResolver;
            _typeLoader = typeLoader;
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
        /// Loads a dependency map using the types in the given <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly that will be used to construct the dependency map.</param>
        /// <returns>A dependency map.</returns>
        public DependencyMap LoadFrom(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            return LoadFrom(new Assembly[] { assembly });
        }

        /// <summary>
        /// Loads a dependency map using the types in the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">The list of assemblies that will be used to construct the dependency map.</param>
        /// <returns>A dependency map.</returns>
        public DependencyMap LoadFrom(IEnumerable<Assembly> assemblies)
        {
            var map = new DependencyMap(_constructorResolver) { Injector = new PropertyInjector() };

            var defaultImplementations = new Dictionary<Type, IImplementation>();
            foreach (var assembly in assemblies)
            {
                var embeddedTypes = _typeLoader.LoadTypes(assembly);
                foreach (var type in embeddedTypes)
                {
                    if (type.IsInterface || type.IsAbstract || type.IsGenericTypeDefinition || type.IsValueType)
                        continue;

                    RegisterNamedFactoryType(type, defaultImplementations, map);
                }
            }

            foreach (var serviceType in defaultImplementations.Keys)
            {
                var dependency = new Dependency(serviceType);
                var implementation = defaultImplementations[serviceType];
                map.AddService(dependency, implementation);
            }

            RegisterServicesFrom(assemblies, map);

            return map;
        }

        /// <summary>
        /// Registers a type as a factory type if it implements the <see cref="IFactory{T}"/> interface.
        /// </summary>
        /// <param name="type">The target type</param>
        /// <param name="defaultImplementations">The list of default implementations per service type.</param>
        /// <param name="map">The dependency map.</param>
        private void RegisterNamedFactoryType(Type type, IDictionary<Type, IImplementation> defaultImplementations, IDependencyMap map)
        {
            var factoryTypeDefinition = typeof(IFactory<>);
            var interfaces = type.GetInterfaces();
            foreach (var interfaceType in interfaces)
            {
                if (!interfaceType.IsGenericType)
                    continue;

                var definitionType = interfaceType.GetGenericTypeDefinition();
                if (definitionType != factoryTypeDefinition)
                    continue;

                var genericArguments = interfaceType.GetGenericArguments();
                var actualServiceType = genericArguments[0];
                var serviceName = type.Name;

                var nameLength = serviceName.Length;
                var hasSpecialName = serviceName.EndsWith("Factory") && nameLength > 7;
                serviceName = hasSpecialName ? serviceName.Substring(0, nameLength - 7) : serviceName;
                var implementation = new FactoryCall(actualServiceType, type.Name);

                // Register the default implementation if necessary
                if (!defaultImplementations.ContainsKey(actualServiceType))
                    defaultImplementations[actualServiceType] = implementation;

                var dependency = new Dependency(actualServiceType, serviceName);
                map.AddService(dependency, implementation);
            }
        }

        /// <summary>
        /// Registers services from the given list of <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">The list of assemblies that contain the service types.</param>
        /// <param name="map">The dependency map.</param>
        private void RegisterServicesFrom(IEnumerable<Assembly> assemblies, DependencyMap map)
        {
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
                foreach (var service in list)
                {
                    if (!filter(service))
                        continue;

                    registeredServices.Add(service);
                }
            }

            map.Register(registeredServices);
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
            if (assemblyLoader == null)
                throw new ArgumentNullException("assemblyLoader");

            // Load the assemblies from the target directory
            var files = Directory.GetFiles(directory, filePattern);
            var assemblies = new List<Assembly>();
            foreach (var file in files)
            {
                var assembly = assemblyLoader.Load(file);

                if (assembly == null)
                    continue;

                assemblies.Add(assembly);
            }

            return LoadFrom(assemblies);
        }

        /// <summary>
        /// Loads a dependency map using the assemblies located in the base application directory.
        /// </summary>
        /// <param name="filePattern">The search pattern that describes which assemblies will be loaded.</param>
        /// <returns>A dependency map.</returns>
        public DependencyMap LoadFromBaseDirectory(string filePattern)
        {
            return LoadFromBaseDirectory(filePattern, new AssemblyLoader());
        }

        /// <summary>
        /// Loads a dependency map using the assemblies located in the base application directory.
        /// </summary>
        /// <param name="filePattern">The search pattern that describes which assemblies will be loaded.</param>
        /// <param name="assemblyLoader">The assembly loader that will load assemblies into memory.</param>
        /// <returns>A dependency map.</returns>
        public DependencyMap LoadFromBaseDirectory(string filePattern, IAssemblyLoader assemblyLoader)
        {
            if (assemblyLoader == null)
                throw new ArgumentNullException("assemblyLoader");

            return LoadFrom(AppDomain.CurrentDomain.BaseDirectory, filePattern, assemblyLoader);
        }

        /// <summary>
        /// Gets the list of default services from a given service list.
        /// </summary>
        /// <param name="serviceList">The list of service implementations that will be used to determine the default service for each service type.</param>
        /// <returns></returns>
        private List<IServiceInfo> GetDefaultServices(IDictionary<Type, IList<IServiceInfo>> serviceList)
        {
            if (serviceList == null)
                throw new ArgumentNullException("serviceList");

            // Get the default services for each service type
            var defaultServices = new List<IServiceInfo>();
            foreach (var serviceType in serviceList.Keys)
            {
                var services = serviceList[serviceType];
                var defaultService = _defaultServiceResolver.GetDefaultService(serviceType, services);

                if (defaultService == null)
                    continue;

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
            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            var serviceList = new HashList<Type, IServiceInfo>();
            foreach (var assembly in assemblies)
            {
                var services = _serviceLoader.Load(assembly);
                foreach (var service in services)
                {
                    var serviceType = service.ServiceType;

                    // Group the services by service type
                    serviceList.Add(serviceType, service);
                }
            }

            return serviceList;
        }
    }
}
