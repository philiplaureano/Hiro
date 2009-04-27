using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using LinFu.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Rrepesnts a class that adds a GetServiceHashCode method to a target type.
    /// </summary>
    public class ServiceHashEmitter
    {
        /// <summary>
        /// Adds a GetServiceHashCode method to a target type.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <param name="shouldBeVisible">A boolean flag that indicates whether or not the method should be public.</param>
        /// <returns>The GetServiceHashCode method.</returns>
        public MethodDefinition AddGetServiceHashMethodTo(TypeDefinition targetType, bool shouldBeVisible)
        {
            var options = new MethodBuilderOptions();
            DefineOptions(targetType, shouldBeVisible, options);

            var module = targetType.Module;

            var methodBuilder = new MethodBuilder();
            var method = methodBuilder.CreateMethod(options);

            var body = method.Body;
            var worker = body.CilWorker;            

            var getHashCodeMethod = module.ImportMethod<object>("GetHashCode");

            var hashVariable = EmitGetServiceTypeHashCode(module, body, worker, getHashCodeMethod);

            var getIsNullOrEmptyMethod = module.ImportMethod<string>("IsNullOrEmpty", BindingFlags.Public | BindingFlags.Static);

            // Calculate the hash code for the service name
            // if it isn't null or empty
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Call, getIsNullOrEmptyMethod);

            var skipNameHash = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Brtrue, skipNameHash);

            EmitGetServiceNameHashCode(worker, getHashCodeMethod, hashVariable);

            worker.Append(skipNameHash);
            worker.Emit(OpCodes.Ldloc, hashVariable);
            worker.Emit(OpCodes.Ret);

            return method;
        }

        /// <summary>
        /// Emits the IL that calculates a hash code from a given service name.
        /// </summary>
        /// <param name="worker">The <see cref="CilWorker"/> that will be used to emit the instructions.</param>
        /// <param name="getHashCodeMethod">The <see cref="Object.GetHashCode"/> method.</param>
        /// <param name="hashVariable">The local variable that will store the hash code.</param>
        private static void EmitGetServiceNameHashCode(CilWorker worker, MethodReference getHashCodeMethod, VariableDefinition hashVariable)
        {
            worker.Emit(OpCodes.Ldloc, hashVariable);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Callvirt, getHashCodeMethod);
            worker.Emit(OpCodes.Xor);
            worker.Emit(OpCodes.Stloc, hashVariable);
        }

        /// <summary>
        /// Emits the IL that calculates a hash code from a given service type.
        /// </summary>
        /// <param name="module">The module that holds the target type.</param>
        /// <param name="body">The body of the GetServiceHashCode method.</param>
        /// <param name="worker">The <see cref="CilWorker"/> that will be used to emit the instructions.</param>
        /// <param name="getHashCodeMethod">The <see cref="Object.GetHashCode"/> method.</param>
        /// <returns>The variable that holds the hash code.</returns>
        private static VariableDefinition EmitGetServiceTypeHashCode(ModuleDefinition module, Mono.Cecil.Cil.MethodBody body, CilWorker worker, MethodReference getHashCodeMethod)
        {
            // Get the hash code for the service type
            var hashVariable = AddLocals(module, body);
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Callvirt, getHashCodeMethod);
            worker.Emit(OpCodes.Stloc, hashVariable);

            return hashVariable;
        }

        /// <summary>
        /// Adds the necessary local variables to the GetServiceHashCode method.
        /// </summary>
        /// <param name="module">The target module.</param>
        /// <param name="body">The method body of the GetServiceHashCode method.</param>
        /// <returns>The variable that holds the hash code.</returns>
        private static VariableDefinition AddLocals(ModuleDefinition module, Mono.Cecil.Cil.MethodBody body)
        {
            var integerType = module.Import(typeof(int));
            var hashVariable = new VariableDefinition(integerType);
            body.Variables.Add(hashVariable);
            body.InitLocals = true;
            body.MaxStack = 3;
            return hashVariable;
        }

        /// <summary>
        /// Sets the default method options for the GetServiceHashCode method.
        /// </summary>
        /// <param name="targetType">The targe type.</param>
        /// <param name="shouldBeVisible">A boolean flag that determines whether or not the method should be publicly visible.</param>
        /// <param name="options">The <see cref="MethodBuilderOptions"/> object to be modified.</param>
        private static void DefineOptions(TypeDefinition targetType, bool shouldBeVisible, MethodBuilderOptions options)
        {
            options.HostType = targetType;
            options.MethodName = "GetServiceHashCode";
            options.ReturnType = typeof(int);
            options.SetMethodParameters(typeof(Type), typeof(string));
            options.IsPublic = shouldBeVisible;
            options.IsStatic = true;
        }
    }
}
