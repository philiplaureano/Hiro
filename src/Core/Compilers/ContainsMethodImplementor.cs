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
            var il = body.GetILProcessor();
            body.InitLocals = true;

            // Remove the stub implementation
            body.Instructions.Clear();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, jumpTargetField);

            // Push the service type
            il.Emit(OpCodes.Ldarg_1);

            // Push the service name
            il.Emit(OpCodes.Ldarg_2);

            // Calculate the hash code using the service type and service name
            il.Emit(OpCodes.Call, getServiceHash);

            var containsEntry = module.ImportMethod<Dictionary<int, int>>("ContainsKey");
            il.Emit(OpCodes.Callvirt, containsEntry);

            var returnValue = containsMethod.AddLocal<bool>();
            il.Emit(OpCodes.Stloc, returnValue);

            var skipCall = il.Create(OpCodes.Nop);
            il.Emit(OpCodes.Ldloc, returnValue);
            il.Emit(OpCodes.Brtrue, skipCall);

            
            var getNextContainer = module.ImportMethod<IMicroContainer>("get_NextContainer");

            
            var otherContainer = containsMethod.AddLocal<IMicroContainer>();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, getNextContainer);
            il.Emit(OpCodes.Stloc, otherContainer);
                        
            // if (otherContainer != null) {
            il.Emit(OpCodes.Ldloc, otherContainer);
            il.Emit(OpCodes.Brfalse, skipCall);

            var otherContainsMethod = module.ImportMethod<IMicroContainer>("Contains");

            // Prevent the container from calling itself 
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc, otherContainer);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue, skipCall);

            // returnValue = otherContainer.Contains(Type, name);
            il.Emit(OpCodes.Ldloc, otherContainer);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Callvirt, otherContainsMethod);
            il.Emit(OpCodes.Stloc, returnValue);

            il.Append(skipCall);
            // }

            il.Emit(OpCodes.Ldloc, returnValue);
            il.Emit(OpCodes.Ret);
        }
    }
}
