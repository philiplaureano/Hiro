using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Compilers;
using Hiro.Interfaces;
using Mono.Cecil;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents a service implementation that will be instantiated as a singleton instance.
    /// </summary>
    public class SingletonType : IImplementation
    {
        /// <summary>
        /// The implementation that will be instantiated as a singleton.
        /// </summary>
        private IImplementation _implementation;

        /// <summary>
        /// Initializes a new instance of the SingletonType class.
        /// </summary>
        /// <param name="implementation">The implementation that will be used to instantiate a service instance.</param>
        public SingletonType(IImplementation implementation)
        {
            _implementation = implementation;
        }

        /// <summary>
        /// Initializes a new instance of the SingletonType class.
        /// </summary>
        /// <param name="targetType">The concrete service type.</param>
        /// <param name="container">The dependency container that contains the dependencies that will be used by the target type.</param>
        public SingletonType(Type targetType, IDependencyContainer container) : this(new TransientType(targetType, container))
        {            
        }

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="targetMethod">The target method.</param>
        public void Emit(IDependency dependency, MethodDefinition targetMethod)
        {
            var emitter = new SingletonEmitter();
            var worker = targetMethod.Body.CilWorker;
            emitter.EmitService(targetMethod, dependency, _implementation);
        }

        /// <summary>
        /// Gets the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>A list of missing dependencies.</returns>
        public IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer map)
        {
            return _implementation.GetMissingDependencies(map);
        }

        /// <summary>
        /// Returns the dependencies that are required by the current implementation.
        /// </summary>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public IEnumerable<IDependency> GetRequiredDependencies()
        {
            return _implementation.GetRequiredDependencies();
        }
    }
}
