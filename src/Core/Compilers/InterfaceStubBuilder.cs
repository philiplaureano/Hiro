using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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
        /// <returns>The list of stubbed methods.</returns>
        public IEnumerable<MethodDefinition> AddStubImplementationFor(Type interfaceType, TypeDefinition type)
        {
            var module = type.Module;
            var interfaceTypeRef = module.Import(interfaceType);
            if (!type.Implements(interfaceTypeRef)) 
                type.Interfaces.Add(interfaceTypeRef);

            return CreateInterfaceStub(interfaceType, type);
        }

        /// <summary>
        /// Overrides all methods in the given interface type with methods that throw a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="interfaceType">The interface type that will be implemented by the target type.</param>
        /// <param name="type">The target type.</param>
        /// <returns>The list of stubbed methods.</returns>
        private static IEnumerable<MethodDefinition> CreateInterfaceStub(Type interfaceType, TypeDefinition type)
        {
            var module = type.Module;
            var overrider = new MethodOverrider();
            var methods = interfaceType.GetMethods();
            var stubbedMethods = new List<MethodDefinition>();
            foreach (var method in methods)
            {
                var newMethod = CreateMethodStub(type, module, overrider, method);
                stubbedMethods.Add(newMethod);
            }

            return stubbedMethods;
        }

        /// <summary>
        /// Overrides the target <paramref name="method"/> with a method that throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="type">The target type.</param>
        /// <param name="module">The host module.</param>
        /// <param name="overrider">The <see cref="MethodOverrider"/> that will be used to override the target method.</param>
        /// <param name="method">The target method.</param>
        /// <returns>The stubbed method.</returns>
        private static MethodDefinition CreateMethodStub(TypeDefinition type, ModuleDefinition module, MethodOverrider overrider, MethodInfo method)
        {
            // Import the NotImplementedException type
            var notImplementedCtor = module.ImportConstructor<NotImplementedException>();
            var currentMethod = overrider.AddOverrideFor(method, type);

            // Create the method stub
            var body = currentMethod.Body;
            var il = body.GetILProcessor();

            il.Emit(OpCodes.Newobj, notImplementedCtor);
            il.Emit(OpCodes.Throw);

            return currentMethod;
        }
    }
}
