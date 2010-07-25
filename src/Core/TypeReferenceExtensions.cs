using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Reflection;

namespace Hiro
{
    /// <summary>
    /// A class that extends the <see cref="TypeDefinition"/>
    /// class with features similar to the features in the
    /// System.Reflection.Emit namespace.
    /// </summary>
    public static class TypeReferenceExtensions
    {
        /// <summary>
        /// Tests if a <see cref="TypeReference"/> is equivalent to an other, no matter
        /// which <see cref="ModuleDefinition"/> defines them.
        /// </summary>
        /// <param name="type">The first type</param>
        /// <param name="otherType">The second type</param>
        /// <returns>True if the types are equivalent, False otherwise</returns>
        public static bool IsEquivalentTo(this TypeReference type, TypeReference otherType)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (otherType == null)
                throw new ArgumentNullException("otherType");

            if (type.FullName != otherType.FullName)
                return false;

            var assembly = GetAssemblyFromScope (type.Scope);
            var otherAssembly = GetAssemblyFromScope (otherType.Scope);

            return assembly.FullName == otherAssembly.FullName;
        }

        /// <summary>
        /// Returns the <see cref="AssemblyNameReference"/> associated with the <see cref="IMetadataScope"/>
        /// </summary>
        /// <param name="scope">A <see cref="IMetadataScope"/></param>
        /// <returns>The associated <see cref="AssemblyNameReference"/></returns>
        private static AssemblyNameReference GetAssemblyFromScope(IMetadataScope scope)
        {
            switch (scope.MetadataScopeType)
            {
                case MetadataScopeType.AssemblyNameReference:
                    return (AssemblyNameReference)scope;
                case MetadataScopeType.ModuleDefinition:
                    return ((ModuleDefinition) scope).Assembly.Name;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
