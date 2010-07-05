using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents a class that creates a <see cref="IMicroContainer"/> implementation.
    /// </summary>
    internal class CreateContainerStub : ICreateContainerType
    {
        /// <summary>
        /// Creates a stub <see cref="IMicroContainer"/> implementation.
        /// </summary>
        /// <param name="typeName">The name of the new container type.</param>
        /// <param name="namespaceName">The namespace of the container type.</param>
        /// <param name="assemblyName">The name of the container assembly.</param>
        /// <returns>A <see cref="TypeDefinition"/> with a stubbed <see cref="IMicroContainer"/> implementation.</returns>
        public TypeDefinition CreateContainerType(string typeName, string namespaceName, string assemblyName)
        {
            var assemblyBuilder = new AssemblyBuilder();
            var assembly = assemblyBuilder.CreateAssembly(assemblyName, ModuleKind.Dll);
            var module = assembly.MainModule;

            var objectType = module.Import(typeof(object));
            var containerInterfaceType = module.Import(typeof(IMicroContainer));
            var typeBuilder = new ContainerTypeBuilder();
            var containerType = typeBuilder.CreateType(typeName, namespaceName, objectType, assembly, containerInterfaceType);

            // Add a stub implementation for the IMicroContainer interface
            var stubBuilder = new InterfaceStubBuilder();
            stubBuilder.AddStubImplementationFor(typeof(IMicroContainer), containerType);

            return containerType;
        }
    }
}
