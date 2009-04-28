using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Interfaces;
using LinFu.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IContainsMethodImplementor"/> class.
    /// </summary>
    internal class ContainsMethodImplementor : IContainsMethodImplementor
    {
        /// <summary>
        /// Emits the body of the <see cref="IMicroContainer.Contains"/> method implementation.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <param name="module">The target module.</param>
        /// <param name="getServiceHash">The method that will be used to determine the hash code of the current service.</param>
        /// <param name="jumpTargetField">The field that contains the list of jump entries.</param>
        public void DefineContainsMethod(TypeDefinition containerType, ModuleDefinition module, MethodDefinition getServiceHash, FieldDefinition jumpTargetField)
        {
            // Override the Contains method stub
            var containsMethod = (from MethodDefinition m in containerType.Methods
                                  where m.Name == "Contains"
                                  select m).First();

            var body = containsMethod.Body;
            var worker = body.CilWorker;

            // Remove the stub implementation
            body.Instructions.Clear();

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldfld, jumpTargetField);

            // Push the service type
            worker.Emit(OpCodes.Ldarg_1);

            // Push the service name
            worker.Emit(OpCodes.Ldarg_2);

            // Calculate the hash code using the service type and service name
            worker.Emit(OpCodes.Call, getServiceHash);

            var containsEntry = module.ImportMethod<Dictionary<int, int>>("ContainsKey");
            worker.Emit(OpCodes.Callvirt, containsEntry);
            worker.Emit(OpCodes.Ret);
        }
    }
}
