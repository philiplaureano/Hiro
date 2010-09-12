using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;

namespace Hiro.Compilers
{
    public class ServiceMapBuilder : ServiceMapBuilder<MethodDefinition>
    {        
    }
    /// <summary>
    /// Represents the default implementation of the <see cref="IServiceMapBuilder"/> interface.
    /// </summary>
    public class ServiceMapBuilder<TMethodBuilder> : IServiceMapBuilder<TMethodBuilder>
    {
        /// <summary>
        /// Gets the list of available services from the given dependency container.
        /// </summary>
        /// <param name="dependencyContainer">The container that holds the application dependencies.</param>
        /// <returns>The service map.</returns>
        public IDictionary<IDependency, IImplementation<TMethodBuilder>> GetAvailableServices(IDependencyContainer<TMethodBuilder> dependencyContainer)
        {
            var serviceMap = new Dictionary<IDependency, IImplementation<TMethodBuilder>>();

            var dependencies = dependencyContainer.Dependencies;
            foreach (var dependency in dependencies)
            {
                var implementations = dependencyContainer.GetImplementations(dependency, false);

                var concreteTypes = new List<IImplementation<TMethodBuilder>>(implementations);

                var implementation = concreteTypes.Count > 0 ? concreteTypes[0] : null;
                if (implementation == null)
                    continue;

                serviceMap[dependency] = implementation;
            }

            return serviceMap;
        }
    }
}
