using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents a class that adds property injection calls to an existing <see cref="IImplementation"/> instance.
    /// </summary>
	public class PropertyInjector : IImplementationInjector
	{
        /// <summary>
        /// Injects the target <paramref name="IImplementation"/> instance.
        /// </summary>
        /// <param name="dependency">The target dependency.</param>
        /// <param name="originalImplementation">The target implementation that will be intercepted by this method.</param>
        /// <returns>The <see cref="IImplementation"/> instance that will be injected in place of the original implementation.</returns>
        public IImplementation Inject(IDependency dependency, IImplementation originalImplementation)
        {
            return new PropertyInjectionCall(originalImplementation);
        }
    }
}
