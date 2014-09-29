using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that can load types into a given dependency map.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Loads types into the given dependency map.
        /// </summary>
        /// <param name="map">The dependency map that will be used to compile the container.</param>
        void Load(DependencyMap map);
    }
}
