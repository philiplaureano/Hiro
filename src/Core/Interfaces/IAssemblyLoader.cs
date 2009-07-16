using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that loads assemblies from a given location.
    /// </summary>
    public interface IAssemblyLoader
    {
        /// <summary>
        /// Loads an assembly from disk.
        /// </summary>
        /// <param name="filename">The file name of the target assembly.</param>
        /// <returns>The loaded <see cref="Assembly"/>.</returns>
        Assembly Load(string filename);
    }
}
