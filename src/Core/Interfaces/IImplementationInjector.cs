using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents an interface that allows users to intercept <see cref="IImplementation"/> instances.
    /// </summary>
    public interface IImplementationInjector
    {
        /// <summary>
        /// Injects the target <paramref name="IImplementation"/> instance.
        /// </summary>
        /// <param name="dependency">The target dependency.</param>
        /// <param name="originalImplementation">The target implementation that will be intercepted by this method.</param>
        /// <returns>The <see cref="IImplementation"/> instance that will be injected in place of the original implementation.</returns>
        IImplementation Inject(IDependency dependency, IImplementation originalImplementation);
    }
}
