using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;

namespace Hiro
{
    /// <summary>
    /// Represents a <see cref="IMicroContainer"/> type that can store object references.
    /// </summary>
    internal class InstanceContainer : IMicroContainer
    {
        private string _serviceName;
        private Type _serviceType;
        private object _instance;

        /// <summary>
        /// Initializes a new instance of the InstanceContainer class.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="instance">The instance that the container will contain.</param>
        public InstanceContainer(Type serviceType, string serviceName, object instance)
        {
            _serviceType = serviceType;
            _serviceName = serviceName;
            _instance = instance;
        }

        /// <summary>
        /// Gets or sets the value indicating the <see cref="IMicroContainer"/> instance that will be added to the current container chain.
        /// </summary>
        /// <value>The next container.</value>
        public IMicroContainer NextContainer
        {
            get;
            set;
        }

        /// <summary>
        /// Determines whether or not the container holds a particular service implementation.
        /// </summary>
        /// <param name="serviceType">The service type to be instantiated.</param>
        /// <param name="key">The name of the service itself.</param>
        /// <returns>A boolean value that specifies whether or not the service exists.</returns>
        public bool Contains(Type serviceType, string key)
        {
            return _serviceType == serviceType && _serviceName == key;
        }

        /// <summary>
        /// Returns all object instances that match the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to be instantiated.</param>
        /// <returns>A list of objects that match the given service type</returns>
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            if (serviceType == _serviceType)
                yield return _serviceType;
        }

        /// <summary>
        /// Obtains an object instance that matches the given <paramref name="serviceType"/>
        /// and <paramref name="key">service name</paramref>.
        /// </summary>
        /// <param name="serviceType">The service type to be instantiated.</param>
        /// <param name="key">The name of the service itself.</param>
        /// <returns>An object instance that matches the given service description.</returns>
        public object GetInstance(Type serviceType, string key)
        {
            if (Contains(serviceType, key))
                return _instance;

            return null;
        }
    }
}