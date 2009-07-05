using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that loads types from a given assembly.
    /// </summary>
    public interface ITypeLoader
    {
        /// <summary>
        /// Loads a set of types from a given assembly.
        /// </summary>
        /// <param name="targetAssembly">The target assembly that contains the types to be loaded.</param>
        /// <returns>The list of types.</returns>
        IEnumerable<Type> LoadTypes(Assembly targetAssembly);
    }
}
