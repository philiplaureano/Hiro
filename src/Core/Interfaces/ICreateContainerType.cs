using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Mono.Cecil;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that can create a <see cref="IMicroContainer"/> implementation.
    /// </summary>
    public interface ICreateContainerType
    {
        /// <summary>
        /// Creates a <see cref="IMicroContainer"/> implementation.
        /// </summary>
        /// <param name="typeName">The name of the new container type.</param>
        /// <param name="namespaceName">The namespace of the container type.</param>
        /// <param name="assemblyName">The name of the container assembly.</param>
        /// <returns>A <see cref="TypeDefinition"/> with a stubbed <see cref="IMicroContainer"/> implementation.</returns>
        TypeDefinition CreateContainerType(string typeName, string namespaceName, string assemblyName);
    }
}
