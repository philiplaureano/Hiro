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
    public class NextContainerCall : BaseContainerCall
    {
        private readonly Type _serviceType;
        private readonly string _serviceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NextContainerCall"/> class.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceName">The service name.</param>
        public NextContainerCall(Type serviceType, string serviceName) : base(serviceType, serviceName)
        {
            _serviceType = serviceType;
            _serviceName = serviceName;
        }

        /// <summary>
        /// Emits the instructions that will obtain the <see cref="IMicroContainer"/> instance.
        /// </summary>
        /// <param name="module">The target module.</param>
        /// <param name="microContainerType">The type reference that points to the <see cref="IMicroContainer"/> type.</param>
        /// <param name="worker">The <see cref="CilWorker"/> that points to the <see cref="IMicroContainer.GetInstance"/> method body.</param>
        /// <param name="skipCreate">The skip label that will be used if the service cannot be instantiated.</param>
        protected override void EmitGetContainerInstance(ModuleDefinition module, TypeReference microContainerType, CilWorker worker, Instruction skipCreate)
        {
            var getNextContainer = module.ImportMethod<IMicroContainer>("get_NextContainer");
            EmitGetNextContainerCall(worker, microContainerType, getNextContainer);
            worker.Emit(OpCodes.Brfalse, skipCreate);

            // var result = NextContainer.GeService(serviceType, serviceName);
            EmitGetNextContainerCall(worker, microContainerType, getNextContainer);
        }

        private void EmitGetNextContainerCall(CilWorker worker, TypeReference microContainerType, MethodReference getNextContainer)
        {
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Isinst, microContainerType);
            worker.Emit(OpCodes.Callvirt, getNextContainer);
        }
    }
}
