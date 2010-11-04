using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;

namespace Hiro.Web
{
    /// <summary>
    /// Represents a helper class that makes it easier to debug cache requests from a <see cref="IMicroContainer"/> instance.
    /// </summary>
    public static class CacheRegistry
    {
        /// <summary>
        /// Obtains a cache instance from the given container.
        /// </summary>
        /// <param name="container">The container that contains the <see cref="ICache"/> instance.</param>
        /// <returns>A cache that contains the services to be cached.</returns>
        public static ICache GetCache(IMicroContainer container)
        {
            var cache = container.GetInstance<ICache>();

            return cache;
        }       
    }
}
