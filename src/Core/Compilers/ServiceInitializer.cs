using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents a type that initializes services using the given service container.
    /// </summary>
    public class ServiceInitializer : IServiceInitializer
    {
        /// <summary>
        /// Emits the instructions that call <see cref="IInitialize.Initialize"/> on a given service instance.
        /// </summary>
        /// <param name="il"></param>
        /// <param name="module">The host module.</param>
        /// <param name="serviceInstance">The local variable that points to the current service instance.</param>
        public void Initialize(ILProcessor il, ModuleDefinition module, VariableDefinition serviceInstance)
        {
            var body = il.Body;
            var method = body.Method;
            var declaringType = method.DeclaringType;

            var targetField = GetTargetField(declaringType);
            if (targetField == null)
                return;

            var initializeType = module.ImportType<IInitialize>();

            il.Emit(OpCodes.Ldloc, serviceInstance);
            il.Emit(OpCodes.Isinst, initializeType);

            var initializeMethod = module.ImportMethod<IInitialize>("Initialize");
            var skipInitializationCall = il.Create(OpCodes.Nop);
            il.Emit(OpCodes.Brfalse, skipInitializationCall);
            
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, targetField);
            GetServiceHash(il, module, serviceInstance);

            var containsMethod = module.ImportMethod<Dictionary<int, int>>("ContainsKey");
            il.Emit(OpCodes.Callvirt, containsMethod);
            il.Emit(OpCodes.Brtrue, skipInitializationCall);

            // if (!__initializedServices.ContainsKey(currentService.GetHashCode()) {
            il.Emit(OpCodes.Ldloc, serviceInstance);
            il.Emit(OpCodes.Isinst, initializeType);
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Callvirt, initializeMethod);

            // __initializedServices.Add(hashCode, 0);            
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, targetField);
            GetServiceHash(il, module, serviceInstance);
            il.Emit(OpCodes.Ldc_I4_1);

            var addMethod = module.ImportMethod<Dictionary<int, int>>("Add");
            il.Emit(OpCodes.Callvirt, addMethod);

            il.Append(skipInitializationCall);
            
        }

        /// <summary>
        /// Emits a call that obtains the hash code for the current service instance.
        /// </summary>
        /// <param name="il">The <see cref="ILProcessor"/> that points to the method body.</param>
        /// <param name="module">The target module.</param>
        /// <param name="serviceInstance">The local variable that contains the service instance.</param>
        private void GetServiceHash(ILProcessor il, ModuleDefinition module, VariableDefinition serviceInstance)
        {
            il.Emit(OpCodes.Ldloc, serviceInstance);

            var getHashCodeMethod = module.ImportMethod<object>("GetHashCode");
            il.Emit(OpCodes.Callvirt, getHashCodeMethod);
        }

        /// <summary>
        /// Searches the <paramref name="declaringType"/> for the initialization field.
        /// </summary>
        /// <param name="declaringType">The target type.</param>
        /// <returns>A field that points to the initialization map.</returns>
        private FieldReference GetTargetField(TypeDefinition declaringType)
        {
            FieldReference targetField = null;
            foreach(FieldDefinition field in declaringType.Fields)
            {
                if (field.Name != "__initializedServices")
                    continue;

                targetField = field;
                break;
            }

            return targetField;
        }
    }
}
