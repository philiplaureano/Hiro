using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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
            var il = body.GetILProcessor ();            

            var getHashCodeMethod = module.ImportMethod<object>("GetHashCode");

            var hashVariable = EmitGetServiceTypeHashCode(module, body, il, getHashCodeMethod);
            
            // Calculate the hash code for the service name
            // if it isn't null
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);

            var skipNameHash = il.Create(OpCodes.Nop);
            il.Emit(OpCodes.Brtrue, skipNameHash);

            EmitGetServiceNameHashCode(il, getHashCodeMethod, hashVariable);

            il.Append(skipNameHash);
            il.Emit(OpCodes.Ldloc, hashVariable);
            il.Emit(OpCodes.Ret);

            return method;
        }

        /// <summary>
        /// Emits the IL that calculates a hash code from a given service name.
        /// </summary>
        /// <param name="il">The <see cref="ILProcessor"/> that will be used to emit the instructions.</param>
        /// <param name="getHashCodeMethod">The <see cref="Object.GetHashCode"/> method.</param>
        /// <param name="hashVariable">The local variable that will store the hash code.</param>
        private static void EmitGetServiceNameHashCode(ILProcessor il, MethodReference getHashCodeMethod, VariableDefinition hashVariable)
        {
            il.Emit(OpCodes.Ldloc, hashVariable);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Callvirt, getHashCodeMethod);
            il.Emit(OpCodes.Xor);
            il.Emit(OpCodes.Stloc, hashVariable);
        }

        /// <summary>
        /// Emits the IL that calculates a hash code from a given service type.
        /// </summary>
        /// <param name="module">The module that holds the target type.</param>
        /// <param name="body">The body of the GetServiceHashCode method.</param>
        /// <param name="il">The <see cref="ILProcessor"/> that will be used to emit the instructions.</param>
        /// <param name="getHashCodeMethod">The <see cref="Object.GetHashCode"/> method.</param>
        /// <returns>The variable that holds the hash code.</returns>
        private static VariableDefinition EmitGetServiceTypeHashCode(ModuleDefinition module, Mono.Cecil.Cil.MethodBody body, ILProcessor il, MethodReference getHashCodeMethod)
        {
            // Get the hash code for the service type
            var hashVariable = AddLocals(module, body);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, getHashCodeMethod);
            il.Emit(OpCodes.Stloc, hashVariable);

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
            body.MaxStackSize = 3;
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
            options.SetMethodParameters(typeof(System.Type), typeof(string));
            options.IsPublic = shouldBeVisible;
            options.IsStatic = true;
        }
    }
}
