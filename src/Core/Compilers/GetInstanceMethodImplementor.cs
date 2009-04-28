using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using LinFu.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IGetInstanceMethodImplementor"/> interface.
    /// </summary>
    internal class GetInstanceMethodImplementor : IGetInstanceMethodImplementor
    {
        #region The GetInstanceMethod implementation
        /// <summary>
        /// Defines the <see cref="IMicroContainer.GetInstance"/> method implementation for the container type.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <param name="module">The target module.</param>
        /// <param name="getServiceHash">The GetServiceHash method.</param>
        /// <param name="jumpTargetField">The field that will store the jump target indexes.</param>
        /// <param name="serviceMap">The service map that contains the list of existing services.</param>
        public void DefineGetInstanceMethod(TypeDefinition containerType, ModuleDefinition module, MethodDefinition getServiceHash, FieldDefinition jumpTargetField, IDictionary<IDependency, IImplementation> serviceMap)
        {
            // Implement the GetInstance method
            var getInstanceMethod = (from MethodDefinition m in containerType.Methods
                                     where m.Name == "GetInstance"
                                     select m).First();

            var body = getInstanceMethod.Body;
            body.InitLocals = true;

            var worker = body.CilWorker;

            body.Instructions.Clear();

            ReturnNullIfServiceDoesNotExist(module, worker);

            var hashVariable = getInstanceMethod.AddLocal<int>();

            // Calculate the service hash code
            EmitCalculateServiceHash(getServiceHash, worker);
            worker.Emit(OpCodes.Stloc, hashVariable);

            EmitJumpTargetIndex(module, jumpTargetField, worker, hashVariable);

            var jumpLabels = DefineJumpLabels(serviceMap, worker);

            DefineServices(serviceMap, getInstanceMethod, worker, jumpLabels);
            worker.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Emits the instructions that will instantiate the target service.
        /// </summary>
        /// <param name="getInstanceMethod">The method that will instantiate the target type.</param>
        /// <param name="dependency">The target dependency</param>
        /// <param name="implementation">The implementation that will instantiate the dependency.</param>
        protected virtual void EmitService(MethodDefinition getInstanceMethod, IDependency dependency, IImplementation implementation)
        {
            implementation.Emit(dependency, getInstanceMethod);
        }

        /// <summary>
        /// Emits the instructions that calculate the hash code of a given service type and service name.
        /// </summary>
        /// <param name="getServiceHash">The method that will be used to calculate the hash code.</param>
        /// <param name="worker">The worker that points to the target method body.</param>
        private static void EmitCalculateServiceHash(MethodDefinition getServiceHash, CilWorker worker)
        {
            // Push the service type
            worker.Emit(OpCodes.Ldarg_1);

            // Push the service name
            worker.Emit(OpCodes.Ldarg_2);

            // Calculate the hash code using the service type and service name
            worker.Emit(OpCodes.Call, getServiceHash);
        }                

        /// <summary>
        /// Emits the instructions that determine which switch label should be executed whenever a particular service name and service type
        /// are pushed onto the stack.
        /// </summary>
        /// <param name="module">The target module.</param>
        /// <param name="jumpTargetField">The field that holds the jump label indexes.</param>
        /// <param name="worker">The <see cref="CilWorker"/> that points to the body of the factory method.</param>
        /// <param name="hashVariable">The local variable that will store the jump index.</param>
        private static void EmitJumpTargetIndex(ModuleDefinition module, FieldDefinition jumpTargetField, CilWorker worker, VariableDefinition hashVariable)
        {
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldfld, jumpTargetField);
            worker.Emit(OpCodes.Ldloc, hashVariable);

            // Calculate the target label index
            var getItem = module.ImportMethod<Dictionary<int, int>>("get_Item");
            worker.Emit(OpCodes.Callvirt, getItem);
        }

        /// <summary>
        /// Defines the jump targets for each service in the <paramref name="serviceMap"/>.
        /// </summary>
        /// <param name="serviceMap">The service map that contains the list of application dependencies.</param>
        /// <param name="worker">The <see cref="CilWorker"/> that points to the body of the factory method.</param>
        /// <returns>A set of jump labels that point to each respective service instantiation operation.</returns>
        private static List<Instruction> DefineJumpLabels(IDictionary<IDependency, IImplementation> serviceMap, CilWorker worker)
        {
            // Define the jump labels
            var jumpLabels = new List<Instruction>();
            var entryCount = serviceMap.Count;
            for (int i = 0; i < entryCount; i++)
            {
                var newLabel = worker.Create(OpCodes.Nop);
                jumpLabels.Add(newLabel);
            }

            return jumpLabels;
        }

        /// <summary>
        /// Emits the instructions that ensure that the target method returns null if the container cannot create the current service name and service type.
        /// </summary>
        /// <param name="module">The target module.</param>
        /// <param name="worker">The worker that points to the method body of the GetInstance method.</param>
        private static void ReturnNullIfServiceDoesNotExist(ModuleDefinition module, CilWorker worker)
        {
            var skipReturnNull = worker.Emit(OpCodes.Nop);
            var containsMethod = module.ImportMethod<IMicroContainer>("Contains");

            // if (!Contains(serviceType, serviceName))
            // return null;
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Ldarg_2);
            worker.Emit(OpCodes.Callvirt, containsMethod);
            worker.Emit(OpCodes.Brtrue, skipReturnNull);

            worker.Emit(OpCodes.Ldnull);
            worker.Emit(OpCodes.Ret);
            worker.Append(skipReturnNull);
        }

        /// <summary>
        /// Defines the instructions that create each service type in the <paramref name="serviceMap"/>.
        /// </summary>
        /// <param name="serviceMap">The service map that contains the list of application dependencies.</param>
        /// <param name="getInstanceMethod">The method that will be used to instantiate the service types.</param>
        /// <param name="worker">The <see cref="CilWorker"/> that points to the body of the factory method.</param>
        /// <param name="jumpLabels">The list of labels that define each service instantiation.</param>
        private void DefineServices(IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition getInstanceMethod, CilWorker worker, List<Instruction> jumpLabels)
        {
            var endLabel = worker.Emit(OpCodes.Nop);

            worker.Emit(OpCodes.Switch, jumpLabels.ToArray());

            var index = 0;
            foreach (var dependency in serviceMap.Keys)
            {
                // Mark the jump label
                var label = jumpLabels[index];
                worker.Append(label);

                // Emit the implementation
                var implementation = serviceMap[dependency];
                EmitService(getInstanceMethod, dependency, implementation);

                worker.Emit(OpCodes.Br, endLabel);
                index++;
            }

            worker.Append(endLabel);
        }
        #endregion
    }
}
