using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IServiceMapBuilder"/> interface.
    /// </summary>
    public class ServiceMapBuilder : IServiceMapBuilder
    {
        /// <summary>
        /// Gets the list of available services from the given dependency container.
        /// </summary>
        /// <param name="dependencyContainer">The container that holds the application dependencies.</param>
        /// <returns>The service map.</returns>
        public IDictionary<IDependency, IImplementation> GetAvailableServices(IDependencyContainer dependencyContainer)
        {
            var serviceMap = new Dictionary<IDependency, IImplementation>();

            var dependencies = dependencyContainer.Dependencies;
            foreach (var dependency in dependencies)
            {
                var implementations = dependencyContainer.GetImplementations(dependency, false);

                var concreteTypes = new List<IImplementation>(implementations);

                var implementation = concreteTypes.Count > 0 ? concreteTypes[0] : null;
                if (implementation == null)
                    continue;

                serviceMap[dependency] = implementation;
            }

            return serviceMap;
        }
    }
}
