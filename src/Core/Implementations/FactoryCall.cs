using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents a class that emits a call to an <see cref="IFactory{T}"/> instance to instantiate a particular service instance.
    /// </summary>
    public class FactoryCall : IImplementation
    {
        private readonly Type _serviceType;
        private readonly string _serviceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryCall"/> class.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceName">The service name.</param>
        public FactoryCall(Type serviceType, string serviceName)
        {
            _serviceType = serviceType;
            _serviceName = serviceName;
        }

        /// <summary>
        /// Gets the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>A list of missing dependencies.</returns>
        public IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer map)
        {
            yield break;
        }

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public IEnumerable<IDependency> GetRequiredDependencies(IDependencyContainer map)
        {
            yield break;
        }

        /// <summary>
        /// Emits the <see cref="IFactory{T}.Create"/> method call that will instantiate the current service instance.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="targetMethod">The target method.</param>
        public void Emit(IDependency dependency, IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition targetMethod)
        {
            var factoryType = typeof (IFactory<>).MakeGenericType(_serviceType);
            var getFactoryInstanceCall = new ContainerCall(factoryType, _serviceName);

            var factoryName = _serviceName;
            getFactoryInstanceCall.Emit(new Dependency(factoryType, factoryName), serviceMap, targetMethod);

            var declaringType = targetMethod.DeclaringType;
            var module = declaringType.Module;            
            var factoryTypeReference = module.Import(factoryType);

            var createMethod = module.Import(factoryType.GetMethod("Create"));

            var IL = targetMethod.GetILGenerator();
            IL.Emit(OpCodes.Isinst, factoryTypeReference);
            IL.Emit(OpCodes.Callvirt, createMethod);
        }
    }
}
