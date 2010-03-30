using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that can create service map instances from a given <see cref="IDependencyContainer"/>.
    /// </summary>
    public interface IServiceMapBuilder
    {
        /// <summary>
        /// Obtains the list of available services from the given <paramref name="dependencyContainer"/>.
        /// </summary>
        /// <param name="dependencyContainer">The container that contains the list of services.</param>
        /// <returns>A dictionary that maps dependencies to their respective implementations.</returns>
        IDictionary<IDependency, IImplementation> GetAvailableServices(IDependencyContainer dependencyContainer);
    }
}
