using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents the basic implementation for a type builder class.
    /// </summary>
    public class TypeBuilder
    {
        /// <summary>
        /// Creates a class type.
        /// </summary>
        /// <param name="typeName">The class name.</param>
        /// <param name="namespaceName">The namespace name.</param>
        /// <param name="baseType">The base type</param>
        /// <param name="assembly">The assembly that will contain the type</param>
        /// <param name="interfaces">The list of interfaces that the type will implement.</param>
        /// <returns>A <see cref="TypeDefinition"/> instance.</returns>
        public virtual TypeDefinition CreateType(string typeName, string namespaceName, TypeReference baseType, AssemblyDefinition assembly, params TypeReference[] interfaces)
        {
            var attributes = TypeAttributes.AutoClass | TypeAttributes.Class |
                             TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

            var module = assembly.MainModule;
            var objectType = module.Import(typeof(object));
            var containerType = module.DefineClass(typeName, namespaceName, attributes, objectType);

            AddInterfaces(module, containerType);

            // Add the default constructor
            containerType.AddDefaultConstructor();

            return containerType;
        }

        /// <summary>
        /// Adds additional interfaces to the target type.
        /// </summary>
        /// <param name="module">The host module.</param>
        /// <param name="containerType">The container type.</param>
        protected virtual void AddInterfaces(ModuleDefinition module, TypeDefinition containerType)
        {
        }
    }
}
