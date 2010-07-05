using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents a class that emits a self container call that instantiates the given service name and service type.
    /// </summary>
    public class ContainerCall : BaseContainerCall 
    {
        /// <summary>
        /// Initializes a new instance of the ContainerCall class.
        /// </summary>
        /// <param name="serviceType">The service type that will be instantiated.</param>
        /// <param name="serviceName">The name of the service to instantiaet.</param>
        public ContainerCall(Type serviceType, string serviceName) : base(serviceType, serviceName)
        {
        }

        /// <summary>
        /// Emits the instructions that will obtain the <see cref="IMicroContainer"/> instance.
        /// </summary>
        /// <param name="module">The target module.</param>
        /// <param name="microContainerType">The type reference that points to the <see cref="IMicroContainer"/> type.</param>
        /// <param name="il">The <see cref="ILProcessor"/> that points to the <see cref="IMicroContainer.GetInstance"/> method body.</param>
        /// <param name="skipCreate">The skip label that will be used if the service cannot be instantiated.</param>
        protected override void EmitGetContainerInstance(ModuleDefinition module, TypeReference microContainerType, ILProcessor il, Instruction skipCreate)
        {
            il.Emit(OpCodes.Ldarg_0);
        }
    }
}
