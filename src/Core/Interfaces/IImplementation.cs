using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Mono.Cecil;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a service implementation that can be emitted in IL.
    /// </summary>
    public interface IImplementation
    {
        /// <summary>
        /// Gets the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>A list of missing dependencies.</returns>
        IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer map);

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        IEnumerable<IDependency> GetRequiredDependencies(IDependencyContainer map);

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="targetMethod">The target method.</param>
        void Emit(IDependency dependency, IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition targetMethod);
    }
}
