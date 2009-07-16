using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Loaders
{
    /// <summary>
    /// Represents a class that determines the default service when multiple implementations of the same type already exist.
    /// </summary>
    public class DefaultServiceResolver : IDefaultServiceResolver
    {
        private IServicePicker _picker;

        /// <summary>
        /// Initializes a new instance of the DefaultServiceResolver class.
        /// </summary>
        public DefaultServiceResolver() : this(new ServicePicker())
        {
        }

        /// <summary>
        /// Initializes a new instance of the DefaultServiceResolver class.
        /// </summary>
        /// <param name="picker">The <see cref="IServicePicker"/> that will determine the default implementation for each service type.</param>
        public DefaultServiceResolver(IServicePicker picker)
        {
            _picker = picker;
        }

        /// <summary>
        /// Determines which service should be used as the default service for the given service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="services">The list of services that implement the service type.</param>
        /// <returns>The <see cref="IServiceInfo"/> instance that will determine </returns>
        public IServiceInfo GetDefaultService(Type serviceType, IEnumerable<IServiceInfo> services)
        {
            IServiceInfo result = null;
            
            result = _picker.ChooseDefaultServiceFrom(serviceType, services);

            return result;
        }        
    }
}
