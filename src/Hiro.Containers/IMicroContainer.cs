using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Containers
{
    /// <summary>
    /// Represents the minimum amount of behavior necessary to implement a service container.
    /// </summary>
    public interface IMicroContainer
    {
        /// <summary>
        /// Gets or sets the value indicating the <see cref="IMicroContainer"/> instance that will be added to the current container chain.
        /// </summary>
        /// <value>The next container.</value>
        IMicroContainer NextContainer { get; set; }

        /// <summary>
        /// Determines whether or not the container holds a particular service implementation.
        /// </summary>
        /// <param name="serviceType">The service type to be instantiated.</param>
        /// <param name="key">The name of the service itself.</param>
        /// <returns>A boolean value that specifies whether or not the service exists.</returns>
        bool Contains(Type serviceType, string key);

        /// <summary>
        /// Returns all object instances that match the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to be instantiated.</param>
        /// <returns>A list of objects that match the given service type</returns>
        IEnumerable<object> GetAllInstances(Type serviceType);

        /// <summary>
        /// Obtains an object instance that matches the given <paramref name="serviceType"/>
        /// and <paramref name="key">service name</paramref>.
        /// </summary>
        /// <param name="serviceType">The service type to be instantiated.</param>
        /// <param name="key">The name of the service itself.</param>
        /// <returns>An object instance that matches the given service description.</returns>
        object GetInstance(Type serviceType, string key);
    }
}
