using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that can map <see cref="IDependency"/> instances to their respective <see cref="IImplementation"/> instances.
    /// </summary>
    public interface IDependencyMap : IDependencyContainer
    {
        /// <summary>
        /// Associates the given <paramref name="implementation"/> with the target <paramref name="dependency"/>.
        /// </summary>
        /// <param name="dependency">The dependency that will be associated with the implementation.</param>
        /// <param name="implementation">The implementation itself.</param>
        void AddService(IDependency dependency, IImplementation implementation);       
    }
}
