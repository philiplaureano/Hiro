using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Describes a service that can be created by the container.
    /// </summary>
    public interface IServiceInfo : IDependency
    {
        /// <summary>
        /// Gets the value indicating the type that will implement the service type.
        /// </summary>
        /// <value>The implementing type.</value>
        Type ImplementingType { get; }
    }
}
