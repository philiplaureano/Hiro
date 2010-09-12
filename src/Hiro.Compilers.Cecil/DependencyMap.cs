using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Compilers.Cecil.Interfaces;
using Hiro.Containers;
using Hiro.Implementations;
using Hiro.Interfaces;
using Hiro.Resolvers;
using Mono.Cecil;

namespace Hiro.Compilers.Cecil
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
                if (combinedEntries.ContainsKey(key))
                    continue;

                combinedEntries.Add(key, rightEntries[key]);
            }

            var map = new DependencyMap(new ConstructorResolver());
            map._entries = combinedEntries;

            return map;
        }

        public DependencyMap()
            : this(new ConstructorResolver())
        {
            ContainerCompiler = new CachedContainerCompiler(new ContainerCompiler());
        }
        public DependencyMap(IConstructorResolver<MethodDefinition> constructorResolver)
            : base(constructorResolver)
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
}
