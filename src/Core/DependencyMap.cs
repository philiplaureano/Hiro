﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hiro.Compilers;
using Hiro.Containers;
using Hiro.Implementations;
using Hiro.Interfaces;
using Hiro.Loaders;
using Hiro.Resolvers;
using Mono.Cecil;
using NGenerics.DataStructures.General;

namespace Hiro
{
    /// <summary>
    /// Represents a class that can map dependencies to implementations.
    /// </summary>
    public class DependencyMap : BaseDependencyMap
    {
        private readonly IConstructorResolver _constructorResolver;

        private readonly Dictionary<AssemblyDefinition, Assembly> _cache =
            new Dictionary<AssemblyDefinition, Assembly>();

        private readonly List<IMicroContainer> _genericContainers = new List<IMicroContainer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyMap"/> class.
        /// </summary>
        /// <param name="constructorResolver">The resolver that will be used to determine the constructor that will be used to instantiate a given object instance.</param>
        public DependencyMap(IConstructorResolver constructorResolver)
            : this(constructorResolver, new CachedContainerCompiler(new ContainerCompiler()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyMap"/> class.
        /// </summary>
        public DependencyMap()
            : this(new ConstructorResolver(), new CachedContainerCompiler(new ContainerCompiler()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyMap"/> class.
        /// </summary>
        /// <param name="constructorResolver">The resolver that will be used to determine the constructor that will be used to instantiate a given object instance.</param>
        /// <param name="compiler">The compiler that will be used to compile this map into an IOC container.</param>
        public DependencyMap(IConstructorResolver constructorResolver, IContainerCompiler compiler)
        {
            
            if (compiler == null)
                throw new ArgumentNullException("compiler");

            if (constructorResolver == null)
                throw new ArgumentNullException("constructorResolver");

            _constructorResolver = constructorResolver;

            ContainerCompiler = compiler;

            // Allow the container to introduce itself to the types that it instantiates
            var dependency = new Dependency(typeof (IMicroContainer));
            AddService(dependency, new ContainerInstanceCall());
        }

        /// <summary>
        /// Gets or sets the value indicating the <see cref="IContainerCompiler"/> that will be used to convert this map into
        /// an IOC container assembly.
        /// </summary>
        /// <value>The container compiler.</value>
        public IContainerCompiler ContainerCompiler
        {
            get;
            set;
        }

        /// <summary>
        /// Merges two dependency maps into a single dependency map.
        /// </summary>
        /// <param name="left">The left-hand dependency map.</param>
        /// <param name="right">The right-hand dependency map.</param>
        /// <returns>A combined dependency map.</returns>
        public static DependencyMap operator +(DependencyMap left, DependencyMap right)
        {
            var leftEntries = left._entries;
            var rightEntries = right._entries;

            // Merge the two entries into a single entry
            var combinedEntries = new HashList<IDependency, IImplementation>();
            foreach (var key in leftEntries.Keys)
            {
                combinedEntries.Add(key, leftEntries[key]);
            }

            foreach (var key in rightEntries.Keys)
            {
                combinedEntries.Add(key, rightEntries[key]);
            }

            var map = new DependencyMap();
            map._entries = combinedEntries;

            return map;
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will implement the service type.</typeparam>
        public void AddService<TService, TImplementation>()
            where TImplementation : TService
        {
            AddService(typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will implement the service type.</typeparam>
        /// <param name="serviceName">The service name.</param>
        public void AddService<TService, TImplementation>(string serviceName)
            where TImplementation : TService
        {
            AddService(serviceName, typeof(TService), typeof(TImplementation));
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public void AddService(string serviceName, System.Type serviceType, System.Type implementingType)
        {
            if (serviceType.IsGenericTypeDefinition && implementingType.IsGenericTypeDefinition)
            {
                throw new NotSupportedException("Named generic service registration is currently not supported");
            }

            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            AddService(new Dependency(serviceType, serviceName), new TransientType(implementingType, this, _constructorResolver));
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public void AddService(System.Type serviceType, System.Type implementingType)
        {
            if (serviceType.IsGenericTypeDefinition && implementingType.IsGenericTypeDefinition)
            {
                var genericContainer = new GenericInstanceContainer(null, serviceType, implementingType, this);
                _genericContainers.Add(genericContainer);
                return;
            }

            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            AddService(new Dependency(serviceType), new TransientType(implementingType, this, _constructorResolver));
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will implement the service type.</typeparam>
        public void AddSingletonService<TService, TImplementation>()
            where TImplementation : TService
        {
            AddService(new Dependency(typeof(TService)), new SingletonType(typeof(TImplementation), this, _constructorResolver));
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will implement the service type.</typeparam>
        /// <param name="serviceName">The service name.</param>
        public void AddSingletonService<TService, TImplementation>(string serviceName)
        {
            AddService(new Dependency(typeof(TService), serviceName), new SingletonType(typeof(TImplementation), this, _constructorResolver));
        }

        /// <summary>
        /// Adds a singleton service to the dependency map.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public void AddSingletonService(System.Type serviceType, System.Type implementingType)
        {
            if (serviceType.IsGenericTypeDefinition && implementingType.IsGenericTypeDefinition)
            {
                var genericContainer = new GenericSingletonInstanceContainer(null, serviceType, implementingType, this);
                _genericContainers.Add(genericContainer);
                return;
            }

            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            AddService(new Dependency(serviceType), new SingletonType(implementingType, this, _constructorResolver));
        }

        /// <summary>
        /// Adds a singleton service to the dependency map.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public void AddSingletonService(string serviceName, System.Type serviceType, System.Type implementingType)
        {
            if (serviceType.IsGenericTypeDefinition && implementingType.IsGenericTypeDefinition)
            {
                throw new NotSupportedException("Named generic service registration is currently not supported");
            }

            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            AddService(new Dependency(serviceType, serviceName), new SingletonType(implementingType, this, _constructorResolver));
        }

        /// <summary>
        /// Adds a deferred service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type that will be injected at runtime.</typeparam>
        /// <param name="serviceName">The service name.</param>
        public void AddDeferredService<TService>(string serviceName)
        {
            AddDeferredService(serviceName, typeof(TService));
        }

        /// <summary>
        /// Adds a deferred service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type that will be injected at runtime.</typeparam>
        public void AddDeferredService<TService>()
        {
            AddDeferredService(typeof(TService));
        }

        /// <summary>
        /// Adds a deferred service to the dependency map.
        /// </summary>
        /// <remarks>This method tells the dependency map that the <paramref name="serviceType"/> will be supplied to the container at runtime.</remarks>
        /// <param name="serviceType">The service type that will be injected at runtime.</param>
        public void AddDeferredService(System.Type serviceType)
        {
            var dependency = new Dependency(serviceType);
            var implementation = new NextContainerCall(serviceType, dependency.ServiceName);
            AddService(dependency, implementation);
        }

        /// <summary>
        /// Adds a deferred service to the dependency map.
        /// </summary>
        /// <remarks>This method tells the dependency map that the <paramref name="serviceType"/> will be supplied to the container at runtime.</remarks>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">The service type that will be injected at runtime.</param>
        public void AddDeferredService(string serviceName, System.Type serviceType)
        {
            var dependency = new Dependency(serviceType, serviceName);
            var implementation = new NextContainerCall(serviceType, serviceName);
            AddService(dependency, implementation);
        }

        /// <summary>
        /// Determines whether or not a service exists within the dependency map.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceName">The service name.</param>
        /// <returns><c>true</c> if the service exists; otherwise, it will return <c>false</c>.</returns>
        public bool Contains(System.Type serviceType, string serviceName)
        {
            return Contains(new Dependency(serviceType, serviceName));
        }

        /// <summary>
        /// Determines whether or not a service exists within the dependency map.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns><c>true</c> if the service exists; otherwise, it will return <c>false</c>.</returns>
        public bool Contains(System.Type serviceType)
        {
            return Contains(new Dependency(serviceType));
        }        

        /// <summary>
        /// Compiles and instantiates a container instance using the current dependencies in the dependency map.
        /// </summary>
        /// <returns>A <see cref="IMicroContainer"/> instance.</returns>
        public IMicroContainer CreateContainer()
        {
            if (ContainerCompiler == null)
                throw new NullReferenceException("The ContainerCompiler property cannot be null");

            var compiler = ContainerCompiler;
            var assembly = compiler.Compile("MicroContainer", "Hiro.Containers", "Hiro.CompiledContainers", this);
            Assembly loadedAssembly = CompileContainerAssembly(assembly);

            var containerTypes = new List<System.Type>();
            foreach (var type in loadedAssembly.GetTypes())
            {
                if (!typeof(IMicroContainer).IsAssignableFrom(type))
                    continue;

                containerTypes.Add(type);
            }

            var containerType = containerTypes[0];
            var result = (IMicroContainer)Activator.CreateInstance(containerType);

            // Append the generic containers
            var containers = new Queue<IMicroContainer>(_genericContainers);

            IMicroContainer iterator = result;
            while (containers.Count > 0)
            {
                var currentContainer = containers.Dequeue();
                if (iterator.NextContainer == null)
                {
                    iterator.NextContainer = currentContainer;
                    iterator = iterator.NextContainer;
                }
            }            
           
            return result;
        }

        /// <summary>
        /// Registers the selected types into the <see cref="DependencyMap"/> from the given assemblies.
        /// </summary>
        /// <param name="assemblies">The list of assemblies that contain the types that will be registered.</param>
        /// <param name="typeFilter">The predicate that determines which types will be registered</param>
        /// <param name="registerTypeAction">The delegate that will be used to register the types.</param>
        public void AddServicesFrom(IEnumerable<Assembly> assemblies, Func<System.Type, bool> typeFilter,
            Action<DependencyMap, System.Type> registerTypeAction)
        {
            var typeLoader = new TypeLoader();
            var allTypes = assemblies.SelectMany(typeLoader.LoadTypes);

            var selectedTypes =
                allTypes.Where(typeFilter).ToArray();

            foreach (var controllerType in selectedTypes)
            {
                registerTypeAction(this, controllerType);
            }
        }

        /// <summary>
        /// Compiles the given <paramref name="assembly">assembly</paramref> definition that contains the compiled service container type.
        /// </summary>
        /// <param name="assembly">The assembly definition that contains the compiled service container type.</param>
        /// <returns>An <see cref="Assembly"/> that contains the compiled service container type.</returns>
        protected virtual Assembly CompileContainerAssembly(AssemblyDefinition assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            Assembly result;
            lock (_cache)
            {
                // Reuse the cached results
                if (_cache.ContainsKey(assembly))
                    return _cache[assembly];

                result = assembly.ToAssembly();

                // Save the cached results
                if (result != null)
                    _cache[assembly] = result;
            }

            return result;
        }

        /// <summary>
        /// A method that tells the dependency map to make a specific <paramref name="serviceType">service type</paramref>
        /// available as a resolvable enumerable service.
        /// </summary>
        /// <param name="serviceType">The service type that should be resolvable as an enumerable service.</param>
        public void AddAsEnumerableService(System.Type serviceType)
        {
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var dependency = new Dependency(enumerableType);
            AddService(dependency, new EnumerableType(serviceType));
        }
    }
}
