using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Implementations
{
    public class FactoryCall : IImplementation
    {
        private readonly Type _serviceType;
        private readonly string _serviceName;

        public FactoryCall(Type serviceType, string serviceName)
        {
            _serviceType = serviceType;
            _serviceName = serviceName;
        }

        public IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer map)
        {
            yield break;
        }

        public IEnumerable<IDependency> GetRequiredDependencies()
        {
            yield break;
        }

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
