using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Loaders
{
    /// <summary>
    /// Represents a class that determines the default service implementation from a list of service implementations.
    /// </summary>
    public class ServicePicker : IServicePicker
    {
        /// <summary>
        /// Determines which <see cref="IServiceInfo"/> instance should be used as the default service.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="services">The list of services.</param>
        /// <returns>The default service implementation.</returns>
        public IServiceInfo ChooseDefaultServiceFrom(Type serviceType, IEnumerable<IServiceInfo> services)
        {
            var serviceTypeName = serviceType.Name;

            IServiceInfo result = null;
            if (!serviceType.IsInterface || !serviceTypeName.StartsWith("I") || serviceTypeName.Length <= 1)
                return null;

            // Match the concrete class with the interface name, e.g. "IService" with "Service"
            var typeName = serviceTypeName.Substring(1);
            foreach (var service in services)
            {
                var implementingType = service.ImplementingType;
                var currentTypeName = implementingType.Name;

                if (currentTypeName != typeName || serviceType != service.ServiceType)
                    continue;

                result = service;
                break;
            }

            if (result != null)
                return result;

            // Return the first item in the list by default
            var serviceList = new List<IServiceInfo>(services);
            if (serviceList.Count >= 1)
                return serviceList[0];

            return result;
        }
    }
}
