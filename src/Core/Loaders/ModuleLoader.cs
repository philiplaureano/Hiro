using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hiro.Interfaces;
using LinFu.Loaders;

namespace Hiro.Loaders
{
    /// <summary>
    /// Represents a type that loads <see cref="IModule"/> instances into memory.
    /// </summary>
    public class ModuleLoader
    {
        private DependencyMap _dependencyMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleLoader"/> class.
        /// </summary>
        /// <param name="dependencyMap">The dependency map that will be configured by the module loader.</param>
        public ModuleLoader(DependencyMap dependencyMap)
        {
            _dependencyMap = dependencyMap;
        }

        /// <summary>
        /// Loads a module into memory so that it can configure a dependency map.
        /// </summary>
        /// <param name="module">
        /// The target module.
        /// </param>
        public void LoadModule(IModule module)
        {
            if(module == null)
                throw new ArgumentNullException("module");

            module.Load(_dependencyMap);
        }

        /// <summary>
        /// Loads modules from the given <paramref name="targetDirectory"/>.
        /// </summary>
        /// <param name="targetDirectory">The target directory that contains the <see cref="IModule"/> assemblies.</param>
        /// <param name="fileSpec">The file search pattern that describes the names of the assemblies that will be loaded.</param>
        public void LoadModulesFrom(string targetDirectory, string fileSpec)
        {
            var modules = new List<IModule>();
            modules.LoadFrom(targetDirectory, fileSpec);

            foreach (var module in modules)
            {
                module.Load(_dependencyMap);
            }
        }
    }
}
