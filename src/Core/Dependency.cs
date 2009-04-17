using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiro
{
    /// <summary>
    /// Represents a service dependency.
    /// </summary>
    public class Dependency : IDependency
    {
        /// <summary>
        /// Initializes a new instance of the Dependency class.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">The service type.</param>
        public Dependency(string serviceName, Type serviceType)
        {
            ServiceName = serviceName;
            ServiceType = serviceType;
        }

        /// <summary>
        /// Gets the value indicating the name of the service itself.
        /// </summary>
        /// <value>The service name.</value>
        public string ServiceName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating the service type.
        /// </summary>
        /// <value>The service type.</value>
        public Type ServiceType
        {
            get;
            private set;
        }
    }
}
