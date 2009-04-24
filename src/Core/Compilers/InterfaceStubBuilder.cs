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
    /// A class that can generate a stub implementation for any given interface type.
    /// </summary>
    public class InterfaceStubBuilder
    {
        /// <summary>
        /// Implements each one of the methods in the given <paramref name="interfaceType"/>.
        /// </summary>
        /// <remarks>By default, each implemented method will throw a <see cref="NotImplementedException"/>.</remarks>
        /// <param name="interfaceType">The interface type.</param>
        /// <param name="type">The host type.</param>
        public void AddStubImplementationFor(Type interfaceType, TypeDefinition type)
        {
            var module = type.Module;            
            var interfaceTypeRef = module.Import(interfaceType);
            var interfaces = type.Interfaces;

            if (!interfaces.Contains(interfaceTypeRef)) 
                interfaces.Add(interfaceTypeRef);

            CreateInterfaceStub(interfaceType, type);
        }

        /// <summary>
        /// Overrides all methods in the given interface type with methods that throw a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="interfaceType">The interface type that will be implemented by the target type.</param>
        /// <param name="type">The target type.</param>
        /// <param name="module">The target module.</param>
        private static void CreateInterfaceStub(Type interfaceType, TypeDefinition type, ModuleDefinition module)
        {
            var overrider = new MethodOverrider();
            var methods = interfaceType.GetMethods();
            foreach (var method in methods)
            {
                CreateMethodStub(type, module, overrider, method);
            }
        }

        /// <summary>
        /// Overrides the target <paramref name="method"/> with a method that throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="type">The target type.</param>
        /// <param name="module">The host module.</param>
        /// <param name="overrider">The <see cref="MethodOverrider"/> that will be used to override the target method.</param>
        /// <param name="method">The target method.</param>
        private static void CreateMethodStub(TypeDefinition type, ModuleDefinition module, MethodOverrider overrider, MethodInfo method)
        {
            // Import the NotImplementedException type
            var notImplementedCtor = module.ImportConstructor<NotImplementedException>();
            var currentMethod = overrider.AddOverrideFor(method, type);

            // Create the method stub
            var body = currentMethod.Body;
            var worker = body.CilWorker;

            worker.Emit(OpCodes.Newobj, notImplementedCtor);
            worker.Emit(OpCodes.Throw);
        }
    }
}
