using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents a class that can create <see cref="AssemblyDefinition"/> instances.
    /// </summary>
    public class AssemblyBuilder
    {
        /// <summary>
        /// Creates an assembly.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <param name="moduleKind">The assembly type.</param>
        /// <returns>An assembly.</returns>
        public AssemblyDefinition CreateAssembly(string assemblyName, ModuleKind moduleKind)
        {
            return AssemblyDefinition.CreateAssembly (new AssemblyNameDefinition (assemblyName, new Version (0, 0, 0, 0)), assemblyName, moduleKind);
        }
    }
}
