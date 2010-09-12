using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents an interface that allows users to intercept <see cref="IImplementation"/> instances.
    /// </summary>
    /// <typeparam name="TMethodBuilder">The method builder type.</typeparam>
    public interface IImplementationInjector<TMethodBuilder>
    {
        /// <summary>
        /// Injects the target <paramref name="originalImplementation"/> instance.
        /// </summary>
        /// <param name="dependency">The target dependency.</param>
        /// <param name="originalImplementation">The target implementation that will be intercepted by this method.</param>
        /// <returns>The <see cref="IImplementation"/> instance that will be injected in place of the original implementation.</returns>
        IImplementation<TMethodBuilder> Inject(IDependency dependency, IImplementation<TMethodBuilder> originalImplementation);
    }
}
