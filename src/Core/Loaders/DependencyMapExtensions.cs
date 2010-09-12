﻿using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Loaders
{
    /// <summary>
    /// Represents a helper class that adds helper methods for loading services into the dependency map.
    /// </summary>
    internal static class DependencyMapExtensions
    {
        /// <summary>
        /// Adds default service implementations to the dependency map.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="defaultServices">The list of default services that will be added to the container as anonymous services.</param>
        internal static void RegisterDefaultServices<TMethodBuilder>(this DependencyMap<TMethodBuilder> map, List<IServiceInfo> defaultServices)
        {
            // Register the default services
            Register(map, defaultServices);
        }

        /// <summary>
        /// Adds named services to the dependency map.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="serviceList">The list of named services that will be added to the container.</param>
        internal static void RegisterNamedServices<TMethodBuilder>(this DependencyMap<TMethodBuilder> map, HashList<Type, IServiceInfo> serviceList)
        {
            // Register the named services
            foreach (var serviceType in serviceList.Keys)
            {
                var services = serviceList[serviceType];
                Register(map, services);
            }
        }

        /// <summary>
        /// Registers a set of services with a dependency map.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="services">The list of services that will be registered with the dependency map.</param>
        internal static void Register<TMethodBuilder>(this DependencyMap<TMethodBuilder> map, IEnumerable<IServiceInfo> services)
        {
            foreach (var service in services)
            {
                Register(map, service);
            }
        }

        /// <summary>
        /// Registers a service with a dependency map.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="service">The service that will be registered with the dependency map.</param>
        internal static void Register<TMethodBuilder>(this DependencyMap<TMethodBuilder> map, IServiceInfo service)
        {
            var serviceName = service.ServiceName;
            var serviceType = service.ServiceType;
            var implementingType = service.ImplementingType;

            map.AddService(serviceName, serviceType, implementingType);
        }
    }
}
