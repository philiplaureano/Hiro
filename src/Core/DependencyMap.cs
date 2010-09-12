using System;
using System.Collections.Generic;
using System.Reflection;
using Hiro.Containers;
using Hiro.Implementations;
using Hiro.Interfaces;
using Hiro.Resolvers;
using Mono.Cecil;
using NGenerics.DataStructures.General;

namespace Hiro
{
    public class DependencyMap : DependencyMap<MethodDefinition>
    {
        private static readonly Dictionary<AssemblyDefinition, Assembly> _cache = new Dictionary<AssemblyDefinition, Assembly>();

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
            var combinedEntries = new HashList<IDependency, IImplementation<MethodDefinition>>();
            foreach (var key in leftEntries.Keys)
            {
                combinedEntries.Add(key, leftEntries[key]);
            }

            foreach (var key in rightEntries.Keys)
            {
                combinedEntries.Add(key, rightEntries[key]);
            }

            var map = new DependencyMap(new ConstructorResolver());
            map._entries = combinedEntries;

            return map;
        }

        public DependencyMap() : this(new ConstructorResolver())
        {
            ContainerCompiler = new CachedContainerCompiler(new ContainerCompiler());
        }
        public DependencyMap(IConstructorResolver<MethodDefinition> constructorResolver) : base(constructorResolver)
        {
            ContainerCompiler = new CachedContainerCompiler(new ContainerCompiler());

            var dependency = new Dependency(typeof(IMicroContainer));
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


        protected override Assembly CompileContainer(IDependencyMap<MethodDefinition> dependencyMap)
        {
            if (ContainerCompiler == null)
                throw new NullReferenceException("The ContainerCompiler property cannot be null");

            var compiler = ContainerCompiler;
            AssemblyDefinition assemblyDefinition = compiler.Compile("MicroContainer", "Hiro.Containers", "Hiro.CompiledContainers", this);
            return CompileContainerAssembly(assemblyDefinition);
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

        protected override IImplementation<MethodDefinition> CreateTransientType(Type implementingType, IConstructorResolver<MethodDefinition> constructorResolver)
        {
            return new TransientType<MethodDefinition>(implementingType, this, constructorResolver);
        }

        protected override IImplementation<MethodDefinition> CreateSingletonType(Type implementingType, IConstructorResolver<MethodDefinition> constructorResolver)
        {            
            return new SingletonType(implementingType, this, constructorResolver);
        }

        protected override IImplementation<MethodDefinition> CreateSingletonType<TImplementation>(IConstructorResolver<MethodDefinition> constructorResolver)
        {
            return CreateSingletonType(typeof(TImplementation), constructorResolver);
        }
        protected override IImplementation<MethodDefinition> CreateNextContainerCall(Type serviceType, Dependency dependency)
        {
            return new NextContainerCall(serviceType, dependency.ServiceName);
        }

    }

    /// <summary>
    /// Represents a class that can map dependencies to implementations.
    /// </summary>
    public abstract class DependencyMap<TMethodBuilder> : BaseDependencyMap<TMethodBuilder>
    {
        private readonly IConstructorResolver<TMethodBuilder> _constructorResolver;

        protected DependencyMap(IConstructorResolver<TMethodBuilder> constructorResolver)
        {
            _constructorResolver = constructorResolver;
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
        public void AddService(string serviceName, Type serviceType, Type implementingType)
        {
            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            AddService(new Dependency(serviceType, serviceName), CreateTransientType(implementingType, _constructorResolver));
        }

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public void AddService(Type serviceType, Type implementingType)
        {
            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            AddService(new Dependency(serviceType), CreateTransientType(implementingType, _constructorResolver));
        }

        /// <summary>
        /// Compiles and instantiates a container instance using the current dependencies in the dependency map.
        /// </summary>
        /// <returns>A <see cref="IMicroContainer"/> instance.</returns>
        public virtual IMicroContainer CreateContainer()
        {         
            Assembly loadedAssembly = CompileContainer(this);

            var containerTypes = new List<Type>();
            foreach (var type in loadedAssembly.GetTypes())
            {
                if (!typeof(IMicroContainer).IsAssignableFrom(type))
                    continue;

                containerTypes.Add(type);
            }

            var containerType = containerTypes[0];
            var result = (IMicroContainer)Activator.CreateInstance(containerType);

            return result;
        }

        protected abstract Assembly CompileContainer(IDependencyMap<TMethodBuilder> dependencyMap);
        protected abstract IImplementation<TMethodBuilder> CreateTransientType(Type implementingType, IConstructorResolver<TMethodBuilder> constructorResolver);
        protected abstract IImplementation<TMethodBuilder> CreateSingletonType(Type implementingType, IConstructorResolver<TMethodBuilder> constructorResolver);
        protected abstract IImplementation<TMethodBuilder> CreateSingletonType<TImplementation>(IConstructorResolver<TMethodBuilder> constructorResolver);
        protected abstract IImplementation<TMethodBuilder> CreateNextContainerCall(Type serviceType, Dependency dependency);        

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will implement the service type.</typeparam>
        public void AddSingletonService<TService, TImplementation>()
            where TImplementation : TService
        {
            AddService(new Dependency(typeof(TService)), CreateSingletonType<TImplementation>(_constructorResolver));
        }

       

        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TImplementation">The concrete type that will implement the service type.</typeparam>
        /// <param name="serviceName">The service name.</param>
        public void AddSingletonService<TService, TImplementation>(string serviceName)
        {
            AddService(new Dependency(typeof(TService), serviceName), CreateSingletonType(typeof(TImplementation), _constructorResolver));
        }

        /// <summary>
        /// Adds a singleton service to the dependency map.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public void AddSingletonService(Type serviceType, Type implementingType)
        {
            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            AddService(new Dependency(serviceType), CreateSingletonType(implementingType, _constructorResolver));
        }        

        /// <summary>
        /// Adds a singleton service to the dependency map.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public void AddSingletonService(string serviceName, Type serviceType, Type implementingType)
        {
            if (!serviceType.IsAssignableFrom(implementingType))
                throw new ArgumentException("The implementing type must be derived from the service type");

            AddService(new Dependency(serviceType, serviceName), CreateSingletonType(implementingType, _constructorResolver));
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
        public void AddDeferredService(Type serviceType)
        {
            var dependency = new Dependency(serviceType);
            var implementation = CreateNextContainerCall(serviceType, dependency);
            AddService(dependency, implementation);
        }        

        /// <summary>
        /// Adds a deferred service to the dependency map.
        /// </summary>
        /// <remarks>This method tells the dependency map that the <paramref name="serviceType"/> will be supplied to the container at runtime.</remarks>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">The service type that will be injected at runtime.</param>
        public void AddDeferredService(string serviceName, Type serviceType)
        {
            var dependency = new Dependency(serviceType, serviceName);
            var implementation = CreateNextContainerCall(serviceType, dependency);
            AddService(dependency, implementation);
        }

        /// <summary>
        /// Determines whether or not a service exists within the dependency map.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceName">The service name.</param>
        /// <returns><c>true</c> if the service exists; otherwise, it will return <c>false</c>.</returns>
        public bool Contains(Type serviceType, string serviceName)
        {
            return Contains(new Dependency(serviceType, serviceName));
        }

        /// <summary>
        /// Determines whether or not a service exists within the dependency map.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <returns><c>true</c> if the service exists; otherwise, it will return <c>false</c>.</returns>
        public bool Contains(Type serviceType)
        {
            return Contains(new Dependency(serviceType));
        }        
    }
}
