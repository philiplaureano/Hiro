using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Describes a service that can be created by the container.
    /// </summary>
    public interface IServiceInfo
    {
        /// <summary>
        /// Gets the value indicating the name of the current service.
        /// </summary>
        /// <value>The name of the service.</value>
        string ServiceName { get; }

        /// <summary>
        /// Gets the value indicating the service type. 
        /// </summary>
        /// <value>The type that describes the service type to be created.</value>
        Type ServiceType { get; }

        /// <summary>
        /// Gets the value indicating the type that will implement the service type.
        /// </summary>
        /// <value>The implementing type.</value>
        Type ImplementingType { get; }
    }
}
