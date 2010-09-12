using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents a class that adds property injection calls to an existing <see cref="IImplementation"/> instance.
    /// </summary>
	public class PropertyInjector : IImplementationInjector<MethodDefinition>
	{
        /// <summary>
        /// Injects the target <paramref name="originalImplementation"/> instance.
        /// </summary>
        /// <param name="dependency">The target dependency.</param>
        /// <param name="originalImplementation">The target implementation that will be intercepted by this method.</param>
        /// <returns>The <see cref="IImplementation"/> instance that will be injected in place of the original implementation.</returns>
        public IImplementation<MethodDefinition> Inject(IDependency dependency, IImplementation<MethodDefinition> originalImplementation)
        {
            var staticImplementation = originalImplementation as IStaticImplementation<ConstructorInfo, MethodDefinition>;

            // HACK: Ignore primitive types by default
            var serviceType = dependency.ServiceType;
            if (serviceType.IsValueType)
                return originalImplementation;

            // Property injection can only be performend on early-bound instantiations
            if (staticImplementation == null)
                return originalImplementation;

            return new PropertyInjectionCall(staticImplementation);
        }
    }
}
