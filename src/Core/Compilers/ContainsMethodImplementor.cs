using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Hiro.Containers;

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
            var targetMethods = new List<MethodDefinition>();
            foreach (MethodDefinition method in containerType.Methods)
            {
                if (method.Name != "Contains")
                    continue;

                targetMethods.Add(method);
            }

            var containsMethod = targetMethods[0];
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

            var returnValue = containsMethod.AddLocal<bool>();
            worker.Emit(OpCodes.Stloc, returnValue);

            var skipCall = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Ldloc, returnValue);
            worker.Emit(OpCodes.Brtrue, skipCall);

            
            var getNextContainer = module.ImportMethod<IMicroContainer>("get_NextContainer");

            
            var otherContainer = containsMethod.AddLocal<IMicroContainer>();
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Callvirt, getNextContainer);
            worker.Emit(OpCodes.Stloc, otherContainer);
                        
            // if (otherContainer != null) {
            worker.Emit(OpCodes.Ldloc, otherContainer);
            worker.Emit(OpCodes.Brfalse, skipCall);

            var otherContainsMethod = module.ImportMethod<IMicroContainer>("Contains");

            // Prevent the container from calling itself 
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldloc, otherContainer);
            worker.Emit(OpCodes.Ceq);
            worker.Emit(OpCodes.Brtrue, skipCall);

            // returnValue = otherContainer.Contains(Type, name);
            worker.Emit(OpCodes.Ldloc, otherContainer);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Ldarg_2);
            worker.Emit(OpCodes.Callvirt, otherContainsMethod);
            worker.Emit(OpCodes.Stloc, returnValue);

            worker.Append(skipCall);
            // }

            worker.Emit(OpCodes.Ldloc, returnValue);
            worker.Emit(OpCodes.Ret);
        }
    }
}
