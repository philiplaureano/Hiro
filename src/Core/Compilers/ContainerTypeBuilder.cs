using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Mono.Cecil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents a class that can create other types.
    /// </summary>
    public class ContainerTypeBuilder : TypeBuilder
    {
        /// <summary>
        /// Adds additional interfaces to the target type.
        /// </summary>
        /// <param name="module">The host module.</param>
        /// <param name="containerType">The container type.</param>
        protected override void AddInterfaces(ModuleDefinition module, TypeDefinition containerType)
        {
            // Implement the IMicroContainer interface
            var microContainerType = module.Import(typeof(IMicroContainer));
            containerType.Interfaces.Add(microContainerType);
        }
    }
}
