using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Containers
{
    /// <summary>
    /// Represents a service dependency.
    /// </summary>
    public class Dependency : IDependency
    {
        /// <summary>
        /// Initializes a new instance of the Dependency class.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        public Dependency(Type serviceType)
            : this(serviceType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Dependency class.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceName">The service name.</param>
        public Dependency(Type serviceType, string serviceName)
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

        /// <summary>
        /// Computes the hash code using the <see cref="ServiceName"/> and <see cref="ServiceType"/>.
        /// </summary>
        /// <returns>The hash code value.</returns>
        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(ServiceName))
                return ServiceType.GetHashCode();

            return ServiceType.GetHashCode() ^ ServiceName.GetHashCode();
        }

        /// <summary>
        /// Determines whether or not the current object is equal to the <paramref name="obj">other object.</paramref>
        /// </summary>
        /// <param name="obj">The object that will be compared with the current object.</param>
        /// <returns><c>true</c> if the objects are equal; otherwise, it will return <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var dependency = obj as Dependency;
            if (obj == null || dependency == null)
                return false;

            if (string.IsNullOrEmpty(ServiceName))
                return ServiceType == dependency.ServiceType;

            return ServiceType == dependency.ServiceType && ServiceName == dependency.ServiceName;
        }

        /// <summary>
        /// Displays the dependency as a string.
        /// </summary>
        /// <returns>A string that displays the contents of the current dependency.</returns>
        public override string ToString()
        {
            var serviceName = string.IsNullOrEmpty(ServiceName) ? "{NoName}" : ServiceName;
            return string.Format("Service Name: {0}, ServiceType: {1}", serviceName, ServiceType.Name);
        }
    }
}