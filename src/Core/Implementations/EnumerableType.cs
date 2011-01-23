using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Compilers;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents an <see cref="IEnumerable{T}"/> dependency call.
    /// </summary>
    public class EnumerableType : IImplementation
    {
        private readonly Type _serviceType;
        private readonly IServiceInitializer _initializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public EnumerableType(Type serviceType) : this(serviceType, new ServiceInitializer())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableType"/> class.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="initializer">The service initializer that will be used to introduce the container to instantiated services.</param>
        public EnumerableType(Type serviceType, IServiceInitializer initializer)
        {
            _serviceType = serviceType;
            _initializer = initializer;
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
            var requiredDependencies = (from d in map.Dependencies
                                       where d.ServiceType == _serviceType
                                       select d).ToList();

            return requiredDependencies;
        }

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="targetMethod">The target method.</param>
        public void Emit(IDependency dependency, IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition targetMethod)
        {            
            var declaringType = targetMethod.DeclaringType;
            var module = declaringType.Module;

            var listType = typeof(List<>).MakeGenericType(_serviceType);
            var listCtor = module.ImportConstructor(listType, new Type[0]);
            
            var listVariable = targetMethod.AddLocal(listType);
            var IL = targetMethod.GetILGenerator();
            IL.Emit(OpCodes.Newobj, listCtor);
            IL.Emit(OpCodes.Stloc, listVariable);

            var targetDependencies = (from d in serviceMap.Keys
                                     where d.ServiceType == _serviceType
                                     select d).ToArray();

            var addItem = module.ImportMethod("Add", listType);

            var serviceType = module.Import(_serviceType);
            var currentService = targetMethod.AddLocal(_serviceType);
            foreach(var currentDependency in targetDependencies)
            {
                IL.Emit(OpCodes.Ldloc, listVariable);

                // Instantiate the current service type
                var implementation = new ContainerCall(currentDependency.ServiceType, currentDependency.ServiceName);
                implementation.Emit(currentDependency, serviceMap, targetMethod);

                IL.Emit(OpCodes.Isinst, serviceType);
                IL.Emit(OpCodes.Stloc, currentService);
                
                // Call IInitialize.Initialize(container) on the current service type
                _initializer.Initialize(IL, module, currentService);

                IL.Emit(OpCodes.Ldloc, currentService);
                IL.Emit(OpCodes.Callvirt, addItem);
            }

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(_serviceType);
            var importedEnumerableType = module.Import(enumerableType);

            IL.Emit(OpCodes.Ldloc, listVariable);
            IL.Emit(OpCodes.Isinst, importedEnumerableType);
        }
    }
}
