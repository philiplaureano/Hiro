using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Loaders
{
    /// <summary>
    /// A class that loads assemblies from a given location.
    /// </summary>
    public class AssemblyLoader : IAssemblyLoader
    {
        /// <summary>
        /// Loads an assembly from disk.
        /// </summary>
        /// <param name="filename">The file name of the target assembly.</param>
        /// <returns>The loaded <see cref="Assembly"/>.</returns>
        public Assembly Load(string filename)
        {
            return Assembly.LoadFrom(filename);
        }
    }
}
