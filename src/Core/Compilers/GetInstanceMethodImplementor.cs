using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using LinFu.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Reflection;

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

            DefineServices(serviceMap, getInstanceMethod, worker);
            worker.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Emits the instructions that will instantiate the target service.
        /// </summary>
        /// <param name="getInstanceMethod">The method that will instantiate the target type.</param>
        /// <param name="dependency">The target dependency</param>       
        /// <param name="implementation">The implementation that will instantiate the dependency.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        protected virtual void EmitService(MethodDefinition getInstanceMethod, IDependency dependency, IImplementation implementation, IDictionary<IDependency, IImplementation> serviceMap)
        {
            implementation.Emit(dependency, serviceMap, getInstanceMethod);
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
        /// Defines the instructions that create each service type in the <paramref name="serviceMap"/>.
        /// </summary>
        /// <param name="serviceMap">The service map that contains the list of application dependencies.</param>
        /// <param name="getInstanceMethod">The method that will be used to instantiate the service types.</param>
        /// <param name="worker">The <see cref="CilWorker"/> that points to the body of the factory method.</param>
        private void DefineServices(IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition getInstanceMethod, CilWorker worker)
        {
            var endLabel = worker.Emit(OpCodes.Nop);

            var body = worker.GetBody();
            body.InitLocals = true;

            var method = body.Method;
            var returnValue = method.AddLocal<object>();
            var declaringType = method.DeclaringType;
            var module = declaringType.Module;

            var getTypeFromHandle = module.ImportMethod<Type>("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
            var equalsMethod = typeof(string).GetMethod("Equals", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(string) }, null);
            var stringEquals = module.Import(equalsMethod);

            foreach (var dependency in serviceMap.Keys)
            {
                var serviceType = module.ImportType(dependency.ServiceType);

                // Match the service type
                worker.Emit(OpCodes.Ldtoken, serviceType);
                worker.Emit(OpCodes.Call, getTypeFromHandle);
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ceq);

                var skipCreate = worker.Create(OpCodes.Nop);
                worker.Emit(OpCodes.Brfalse, skipCreate);

                // Match the service name
                var serviceName = dependency.ServiceName;

                worker.Emit(OpCodes.Ldarg_2);

                // Push the service name onto the stack
                var pushName = serviceName == null
                                   ? worker.Create(OpCodes.Ldnull)
                                   : worker.Create(OpCodes.Ldstr, serviceName);

                worker.Append(pushName);
                worker.Emit(OpCodes.Call, stringEquals);

                worker.Emit(OpCodes.Brfalse, skipCreate);

                // Emit the implementation
                var implementation = serviceMap[dependency];
                EmitService(getInstanceMethod, dependency, implementation, serviceMap);

                if (serviceType.IsValueType)
                    worker.Emit(OpCodes.Box, serviceType);

                worker.Emit(OpCodes.Br, endLabel);

                // Fall through to the next if-then-else case
                worker.Append(skipCreate);
            }

            worker.Emit(OpCodes.Ldnull);
            worker.Append(endLabel);
        }
        #endregion
    }
}
