using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a service implementation that can be emitted in IL.
    /// </summary>
    /// <typeparam name="TMethodBuilder">The method builder type.</typeparam>
    public interface IImplementation<TMethodBuilder>
    {
        /// <summary>
        /// Gets the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>A list of missing dependencies.</returns>
        IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer<TMethodBuilder> map);

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        IEnumerable<IDependency> GetRequiredDependencies(IDependencyContainer<TMethodBuilder> map);

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="targetMethod">The target method.</param>
        void Emit(IDependency dependency, IDictionary<IDependency, IImplementation<TMethodBuilder>> serviceMap, TMethodBuilder targetMethod);
    }
}
