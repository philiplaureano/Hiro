using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Containers
{
    /// <summary>
    /// Represents a service dependency.
    /// </summary>
    public interface IDependency
    {
        /// <summary>
        /// Gets the value indicating the name of the service itself.
        /// </summary>
        /// <value>The service name.</value>
        string ServiceName { get; }

        /// <summary>
        /// Gets a value indicating the service type.
        /// </summary>
        /// <value>The service type.</value>
        Type ServiceType { get; }
    }
}