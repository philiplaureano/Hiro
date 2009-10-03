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
    /// Represents an implementation that will use the next container in the <see cref="IMicroContainer"/>
    /// chain to instantiate a particular service name and service type.
    /// </summary>
    public class NextContainerCall : IImplementation
    {
        private readonly Type _serviceType;
        private readonly string _serviceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NextContainerCall"/> class.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceName">The service name.</param>
        public NextContainerCall(Type serviceType, string serviceName)
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
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public IEnumerable<IDependency> GetRequiredDependencies()
        {
            yield break;
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

            var microContainerType = module.ImportType<IMicroContainer>();
            var getNextContainer = module.ImportMethod<IMicroContainer>("get_NextContainer");

            var worker = targetMethod.GetILGenerator();

            // if (this is IMicroContainer && this.NextContainer != null) {
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Isinst, microContainerType);

            var skipCreate = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Brfalse, skipCreate);

            EmitGetNextContainerCall(worker, microContainerType, getNextContainer);
            worker.Emit(OpCodes.Brfalse, skipCreate);

            // var result = NextContainer.GeService(serviceType, serviceName);
            EmitGetNextContainerCall(worker, microContainerType, getNextContainer);

            var getInstance = module.ImportMethod<IMicroContainer>("GetInstance");
            var getTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var getTypeFromHandle = module.Import(getTypeFromHandleMethod);

            // Push the service type onto the stack
            var serviceType = module.Import(_serviceType);
            worker.Emit(OpCodes.Ldtoken, serviceType);
            worker.Emit(OpCodes.Call, getTypeFromHandle);

            var loadString = string.IsNullOrEmpty(_serviceName)
                                 ? worker.Create(OpCodes.Ldnull)
                                 : worker.Create(OpCodes.Ldstr, _serviceName);

            worker.Append(loadString);
            worker.Emit(OpCodes.Callvirt, getInstance);

            var endLabel = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Br, endLabel);

            worker.Append(skipCreate);


            var serviceNotFoundExceptionCtor = module.ImportConstructor<ServiceNotFoundException>(typeof(string),
                                                                                                  typeof(Type));
            var serviceName = dependency.ServiceName ?? string.Empty;
            worker.Emit(OpCodes.Ldstr, serviceName);
            worker.Emit(OpCodes.Ldtoken, serviceType);
            worker.Emit(OpCodes.Call, getTypeFromHandle);
            
            worker.Emit(OpCodes.Newobj, serviceNotFoundExceptionCtor);
            worker.Emit(OpCodes.Throw);

            worker.Append(endLabel);

            // }
        }

        private void EmitGetNextContainerCall(CilWorker worker, TypeReference microContainerType, MethodReference getNextContainer)
        {
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Isinst, microContainerType);
            worker.Emit(OpCodes.Callvirt, getNextContainer);
        }
    }
}
