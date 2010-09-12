using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Compilers;
using Hiro.Containers;
using Hiro.Interfaces;
using Hiro.Resolvers;
using Mono.Cecil;

namespace Hiro.Implementations
{
    public class SingletonType : SingletonType<MethodDefinition>
    {
        public SingletonType(IStaticImplementation<ConstructorInfo, MethodDefinition> implementation, ISingletonEmitter<MethodDefinition> singletonEmitter)
            : base(implementation, singletonEmitter)
        {
        }

        public SingletonType(Type targetType, IDependencyContainer<MethodDefinition> container, IConstructorResolver<MethodDefinition> constructorResolver, ISingletonEmitter<MethodDefinition> singletonEmitter) : base(targetType, container, constructorResolver, singletonEmitter)
        {
        }

        public SingletonType(Type targetType, IDependencyContainer<MethodDefinition> container, IConstructorResolver<MethodDefinition> constructorResolver) : base(targetType, container, constructorResolver)
        {
        }

        protected override IStaticImplementation<ConstructorInfo, MethodDefinition> CreateTransientType(Type targetType, 
            IDependencyContainer<MethodDefinition> container, IConstructorResolver<MethodDefinition> constructorResolver)
        {
            return new TransientType<MethodDefinition>(targetType, container, constructorResolver);
        }

        protected override ISingletonEmitter<MethodDefinition> CreateContainerBasedSingletonEmitter()
        {
            return new ContainerBasedSingletonEmitter();
        }
    }
    /// <summary>
    /// Represents a service implementation that will be instantiated as a singleton instance.
    /// </summary>
    public abstract class SingletonType<TMethodBuilder> : IStaticImplementation<Type, TMethodBuilder>
    {
        /// <summary>
        /// The implementation that will be instantiated as a singleton.
        /// </summary>
        private readonly IStaticImplementation<ConstructorInfo, TMethodBuilder> _implementation;

        /// <summary>
        /// The singleton emitter that will generate the singleton types.
        /// </summary>
        private readonly ISingletonEmitter<TMethodBuilder> _emitter;
        
        /// <summary>
        /// Initializes a new instance of the SingletonType class.
        /// </summary>
        /// <param name="singletonEmitter">The emitter that will be responsible for instantiating the singleton implementation.</param>
        /// <param name="implementation">The implementation that will be used to emitting a service instance.</param>
        public SingletonType(IStaticImplementation<ConstructorInfo, TMethodBuilder> implementation, ISingletonEmitter<TMethodBuilder> singletonEmitter)
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
        public SingletonType(Type targetType, IDependencyContainer<TMethodBuilder> container, IConstructorResolver<TMethodBuilder> constructorResolver, 
            ISingletonEmitter<TMethodBuilder> singletonEmitter)
        {
            _implementation = CreateTransientType(targetType, container, constructorResolver);
            _emitter = singletonEmitter;            
        }       

        /// <summary>
        /// Initializes a new instance of the SingletonType class.
        /// </summary>
        /// <param name="targetType">The concrete service type.</param>
        /// <param name="container">The dependency container that contains the dependencies that will be used by the target type.</param>
        /// <param name="constructorResolver">The constructor resolver.</param>
        public SingletonType(Type targetType, IDependencyContainer<TMethodBuilder> container, IConstructorResolver<TMethodBuilder> constructorResolver)
        {
            _implementation = CreateTransientType(targetType, container, constructorResolver);
            _emitter = CreateContainerBasedSingletonEmitter(); 
        }        

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="targetMethod">The target method.</param>
        public void Emit(IDependency dependency, IDictionary<IDependency, IImplementation<TMethodBuilder>> serviceMap, TMethodBuilder targetMethod)
        {
            _emitter.EmitService(targetMethod, dependency, _implementation, serviceMap);
        }

        /// <summary>
        /// Gets the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>A list of missing dependencies.</returns>
        public IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer<TMethodBuilder> map)
        {
            return _implementation.GetMissingDependencies(map);
        }

        /// <summary>
        /// Returns the dependencies that are required by the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public IEnumerable<IDependency> GetRequiredDependencies(IDependencyContainer<TMethodBuilder> map)
        {
            return _implementation.GetRequiredDependencies(map);
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

        public Type Target
        {
            get { return _implementation.TargetType; }
        }

        protected abstract IStaticImplementation<ConstructorInfo, TMethodBuilder> CreateTransientType(Type targetType, IDependencyContainer<TMethodBuilder> container,
                                IConstructorResolver<TMethodBuilder> constructorResolver);
        protected abstract ISingletonEmitter<TMethodBuilder> CreateContainerBasedSingletonEmitter();




        
    }
}
