using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that determines the default service implementation from a given list of services.
    /// </summary>
    public interface IServicePicker
    {
        /// <summary>
        /// Determines which <see cref="IServiceInfo"/> instance should be used as the default service.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="services">The list of services.</param>
        /// <returns>The default service implementation.</returns>
        IServiceInfo ChooseDefaultServiceFrom(Type serviceType, IEnumerable<IServiceInfo> services);
    }
}
