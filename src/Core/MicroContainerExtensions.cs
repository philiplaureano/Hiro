using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;

namespace Hiro
{
    /// <summary>
    /// A helper class that adds syntactic sugar to the <see cref="IMicroContainer"/> interface.
    /// </summary>
    public static class MicroContainerExtensions
    {
        /// <summary>
        /// Obtains an object instance that matches the given service type
        /// and <paramref name="key">service name</paramref>.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// <param name="container">The target container.</param>
        /// <returns>An object instance that matches the given service description.</returns>
        public static T GetInstance<T>(this IMicroContainer container)
        {
            return container.GetInstance<T>(null);
        }

        /// <summary>
        /// Obtains an object instance that matches the given service type
        /// and <paramref name="key">service name</paramref>.
        /// </summary>
        /// <typeparam name="T">The service type.</typeparam>
        /// /// <param name="container">The target container.</param>
        /// <param name="key">The name of the service itself.</param>
        /// <returns>An object instance that matches the given service description.</returns>
        public static T GetInstance<T>(this IMicroContainer container, string key)
        {
            return (T)container.GetInstance(typeof(T), key);
        }
    }
}
