using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Interfaces;

namespace Hiro
{
    /// <summary>
    /// Describes a service that can be created by the container.
    /// </summary>
    public class ServiceInfo : IServiceInfo
    {
        /// <summary>
        /// Initializes the class with the given <paramref name="serviceName"/>
        /// and <paramref name="serviceType"/>.
        /// </summary>        
        /// <param name="serviceType">The type of service that can be created.</param>
        /// <param name="implementingType">The type that will implement the service type.</param>
        /// <param name="serviceName">The name of the service.</param>
        public ServiceInfo(Type serviceType, Type implementingType, string serviceName)
        {
            ServiceType = serviceType;
            ServiceName = serviceName;
            ImplementingType = implementingType;
        }

        /// <summary>
        /// Gets the value indicating the name of the current service.
        /// </summary>
        /// <value>The name of the service.</value>
        public string ServiceName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the value indicating the service type. 
        /// </summary>
        /// <value>The type that describes the service type to be created.</value>
        public Type ServiceType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the value indicating the type that will implement the service type.
        /// </summary>
        /// <value>The implementing type.</value>
        public Type ImplementingType
        {
            get;
            set;
        }

        /// <summary>
        /// Determines if the other object is equal to the current <see cref="IServiceInfo"/> instance.
        /// </summary>
        /// <param name="obj">The other object that will be used in the comparison.</param>
        /// <returns>Returns <c>true</c> if both instances have the same service name, implement the same service type and have the same arguments; otherwise, it will return <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is IServiceInfo))
                return false;

            var other = (IServiceInfo)obj;
            return GetHashCode() == other.GetHashCode();
        }

        /// <summary>
        /// Calculates the hash code using the service name and service type.
        /// </summary>
        /// <returns>The service hash code.</returns>
        public override int GetHashCode()
        {
            int hash = 0;

            // Hash the service name
            if (!string.IsNullOrEmpty(ServiceName))
                hash = ServiceName.GetHashCode();

            // Hash the service type
            hash ^= ServiceType.GetHashCode();

            // Hash the implementing type
            hash ^= ImplementingType.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            var serviceName = ServiceName ?? "{null}";
            return string.Format("Service Name: {0}, Service Type: {1}, ImplementingType {2}", serviceName,
                                 ServiceType.FullName, ImplementingType.FullName);
        }
    }
}
