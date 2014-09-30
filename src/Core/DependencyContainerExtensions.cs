using System.Collections.Generic;
using Hiro.Compilers;
using Hiro.Containers;
using Hiro.Interfaces;

namespace Hiro
{
    /// <summary>
    /// Represents a helper that adds helper methods to the <see cref="IDependencyContainer"/> interface.
    /// </summary>
    public static class DependencyContainerExtensions
    {
        /// <summary>
        /// Creates a service map that lists the available services in the current dependency map.
        /// </summary>
        /// <param name="container">
        /// The dependency container.
        /// </param>
        /// <returns>
        /// A list of the available services in the current dependency map.
        /// </returns>
        public static IDictionary<IDependency, IImplementation> GetServiceMap(this IDependencyContainer container)
        {
            var mapBuilder = new ServiceMapBuilder();
            return mapBuilder.GetAvailableServices(container);
        }
    }
}