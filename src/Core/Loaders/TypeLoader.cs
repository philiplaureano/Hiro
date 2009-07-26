using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Loaders
{
    /// <summary>
    /// Represents a type that loads types from a given assembly.
    /// </summary>
    public class TypeLoader : ITypeLoader
    {
        /// <summary>
        /// Loads a set of types from a given assembly.
        /// </summary>
        /// <param name="targetAssembly">The target assembly that contains the types to be loaded.</param>
        /// <returns>The list of types.</returns>
        public IEnumerable<Type> LoadTypes(Assembly targetAssembly)
        {
            IEnumerable<Type> loadedTypes = null;
            try
            {
                loadedTypes = targetAssembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                loadedTypes = ex.Types;
            }

            return loadedTypes;
        }
    }
}
