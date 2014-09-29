using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Containers;
using Hiro.Implementations;
using Hiro.Resolvers;

namespace Hiro.Web
{
    /// <summary>
    /// Represents a helper class that adds web extension methods to the <see cref="DependencyMap"/> class.
    /// </summary>
    public static class DependencyMapExtensions
    {
        /// <summary>
        /// Adds a service to the dependency map.
        /// </summary>
        /// <remarks>This service will be created once per web session.</remarks>
        /// <param name="map">The target dependency map.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public static void AddPerSessionService(this DependencyMap map, System.Type serviceType, System.Type implementingType)
        {
            map.AddPerSessionService(null, serviceType, implementingType);
        }

        /// <summary>
        /// Adds a named service to the dependency map.
        /// </summary>
        /// <remarks>This service will be created once per web session.</remarks>
        /// <param name="map">The target dependency map.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementingType">The implementing type.</param>
        public static void AddPerSessionService(this DependencyMap map, string serviceName, System.Type serviceType, System.Type implementingType)
        {
            // Add the SessionCache by default
            if (!map.Contains(typeof(ICache)))
                map.AddService<ICache, SessionCache>();

            // Add the HttpReferenceTracker by default
            if(!map.Contains(typeof(IHttpReferenceTracker)))
                map.AddService<IHttpReferenceTracker, HttpReferenceTracker>();

            // The cached instantiation class will use the cache in the container
            // to cache service instances
            var dependency = new Dependency(serviceType, serviceName);
            var implementation = new CachedInstantiation(new TransientType(implementingType, map, new ConstructorResolver()));

            map.AddService(dependency, implementation);
        }        
    }
}
