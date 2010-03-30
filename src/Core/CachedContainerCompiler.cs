using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Interfaces;
using Mono.Cecil;

namespace Hiro
{
    /// <summary>
    /// Represents a container compiler that caches its compiled results.
    /// </summary>
    internal class CachedContainerCompiler : IContainerCompiler
    {
        private readonly IContainerCompiler _compiler;
        private readonly Dictionary<int, AssemblyDefinition> _cache = new Dictionary<int, AssemblyDefinition>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedContainerCompiler"/> class.
        /// </summary>
        /// <param name="compiler">The container compiler implementation.</param>
        internal CachedContainerCompiler(IContainerCompiler compiler)
        {
            if (compiler == null)
                throw new ArgumentNullException("compiler");

            _compiler = compiler;
        }

        /// <summary>
        /// Compiles a dependency graph into an IOC container.
        /// </summary>
        /// <param name="dependencyContainer">The <see cref="IDependencyContainer"/> instance that contains the services that will be instantiated by compiled container.</param>
        /// <returns>An assembly containing the compiled IOC container.</returns>
        public AssemblyDefinition Compile(string typeName, string namespaceName, string assemblyName, IDependencyContainer dependencyContainer)
        {
            var hash = dependencyContainer.GetHashCode();

            AssemblyDefinition result;
            lock (_cache)
            {
                if (_cache.ContainsKey(hash))
                    return _cache[hash];


                // Cache the result
                result = _compiler.Compile("MicroContainer", "Hiro.Containers", "Hiro.CompiledContainers", dependencyContainer);

                _cache[hash] = result;
            }

            return result;
        }
    }
}
