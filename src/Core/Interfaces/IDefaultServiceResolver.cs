using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that determines the default service when multiple implementations of the same type already exist in the dependency map.
    /// </summary>
    public interface IDefaultServiceResolver
    {
        /// <summary>
        /// Determines which service should be used as the default service for the given service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="services">The list of services that implement the service type.</param>
        /// <returns>The <see cref="IServiceInfo"/> instance that will determine </returns>
        IServiceInfo GetDefaultService(Type serviceType, IEnumerable<IServiceInfo> services);
    }
}
