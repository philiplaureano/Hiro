using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that can load services from a given assembly.
    /// </summary>
    public interface IServiceLoader
    {
        /// <summary>
        /// Loads services from the given assembly.
        /// </summary>
        /// <param name="targetAssembly">The assembly that contains the types to be loaded.</param>
        /// <returns>The list of services.</returns>
        IEnumerable<IServiceInfo> Load(Assembly targetAssembly);
    }
}
