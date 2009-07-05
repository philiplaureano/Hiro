using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Loaders
{
    public class TypeLoader : ITypeLoader
    {
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
