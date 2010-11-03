using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Compilers;
using Hiro.Containers;
using Hiro.Interfaces;
using Hiro.Resolvers;
using Mono.Cecil;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents a service implementation that will be instantiated as a singleton instance.
    /// </summary>
    public class SingletonType : IStaticImplementation
    {
        /// <summary>
        /// The implementation that will be instantiated as a singleton.
        /// </summary>
        private readonly IStaticImplementation _implementation;

        /// <summary>
        /// The singleton emitter that will generate the singleton types.
        /// </summary>
        private readonly ISingletonEmitter _emitter = new ContainerBasedSingletonEmitter();

        /// <summary>
        /// Initializes a new instance of the SingletonType class.
        /// </summary>
        /// <param name="singletonEmitter">The emitter that will be responsible for instantiating the singleton implementation.</param>
        /// <param name="implementation">The implementation that will be used to emitting a service instance.</param>
        public SingletonType(IStaticImplementation implementation, ISingletonEmitter singletonEmitter)
        {
            _implementation = implementation;
            _emitter = singletonEmitter;
        }

        /// <summary>
        /// Initializes a new instance of the SingletonType class.
        /// </summary>
        /// <param name="targetType">The concrete service type.</param>
        /// <param name="container">The dependency container that contains the dependencies that will be used by the target type.</param>
        /// <param name="constructorResolver">The constructor resolver.</param>
        /// <param name="singletonEmitter">The emitter that will be responsible for emitting the singleton implementation.</param>
        public SingletonType(Type targetType, IDependencyContainer container, IConstructorResolver constructorResolver, ISingletonEmitter singletonEmitter)
            : this(new TransientType(targetType, container, constructorResolver), singletonEmitter)
        {            
        }

        /// <summary>
        /// Initializes a new instance of the SingletonType class.
        /// </summary>
        /// <param name="targetType">The concrete service type.</param>
        /// <param name="container">The dependency container that contains the dependencies that will be used by the target type.</param>
        /// <param name="constructorResolver">The constructor resolver.</param>
        public SingletonType(Type targetType, IDependencyContainer container, IConstructorResolver constructorResolver)
            : this(new TransientType(targetType, container, constructorResolver), new ContainerBasedSingletonEmitter())
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
        /// <param name="map">The implementation map.</param>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public IEnumerable<IDependency> GetRequiredDependencies(IDependencyContainer map)
        {
            return _implementation.GetRequiredDependencies(map);
        }
    }
}
