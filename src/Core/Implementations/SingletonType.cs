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
        /// The singleton emitter that will generate the singleton types.
        /// </summary>
        private SingletonEmitter _emitter = new SingletonEmitter();

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
        public SingletonType(Type targetType, IDependencyContainer container)
            : this(new TransientType(targetType, container))
        {
        }

        /// <summary>
        /// Gets the value indicating the type that will be instantiated by this implementation.
        /// </summary>
        /// <value>The target type.</value>
        public Type TargetType
        {
            get
            {
                return _implementation.TargetType;
            }
        }

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="targetMethod">The target method.</param>
        public void Emit(IDependency dependency, IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition targetMethod)
        {
            var worker = targetMethod.Body.CilWorker;
            _emitter.EmitService(targetMethod, dependency, _implementation, serviceMap);
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
