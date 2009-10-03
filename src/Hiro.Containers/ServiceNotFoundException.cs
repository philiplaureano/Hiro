using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Containers
{
    /// <summary>
    /// Represents the exception thrown when a particular service cannot be resolved by a <see cref="IMicroContainer"/> instance.
    /// </summary>
    public class ServiceNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNotFoundException"/> class.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">The service type.</param>
        public ServiceNotFoundException(string serviceName, Type serviceType)
        {
            ServiceName = serviceName;
            ServiceType = serviceType;
        }

        /// <summary>
        /// Gets the value of the service name.
        /// </summary>
        /// <value>The name of the service.</value>
        public string ServiceName
        {
            get; 
            private set;
        }

        /// <summary>
        /// Gets the value of the service type.
        /// </summary>
        /// <value>The service type.</value>
        public Type ServiceType
        {
            get; 
            private set;
        }
    }
}
