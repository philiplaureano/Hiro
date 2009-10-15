using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Hiro.Containers;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents a class that provides the basic fuctionality for a compiled <see cref="IMicroContainer"/> instance to compile itself.
    /// </summary>
    public abstract class BaseContainerCall : IImplementation
    {
        private readonly Type _serviceType;
        private readonly string _serviceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NextContainerCall"/> class.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceName">The service name.</param>
        protected BaseContainerCall(Type serviceType, string serviceName)
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


            var worker = targetMethod.GetILGenerator();

            // if (this is IMicroContainer && this.NextContainer != null) {
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Isinst, microContainerType);

            var skipCreate = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Brfalse, skipCreate);

            EmitGetContainerInstance(module, microContainerType, worker, skipCreate);

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

        /// <summary>
        /// Emits the instructions that will obtain the <see cref="IMicroContainer"/> instance.
        /// </summary>
        /// <param name="module">The target module.</param>
        /// <param name="microContainerType">The type reference that points to the <see cref="IMicroContainer"/> type.</param>
        /// <param name="worker">The <see cref="CilWorker"/> that points to the <see cref="IMicroContainer.GetInstance"/> method body.</param>
        /// <param name="skipCreate">The skip label that will be used if the service cannot be instantiated.</param>
        protected abstract void EmitGetContainerInstance(ModuleDefinition module, TypeReference microContainerType, CilWorker worker, Instruction skipCreate);        
    }
}
