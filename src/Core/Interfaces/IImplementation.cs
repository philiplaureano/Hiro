using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        IEnumerable<IDependency> GetRequiredDependencies();

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation.
        /// </summary>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of available dependencies in the current application.</param>
        void Emit(MethodDefinition targetMethod, IDependency dependency, IDictionary<IDependency, IImplementation> serviceMap);
    }
}
