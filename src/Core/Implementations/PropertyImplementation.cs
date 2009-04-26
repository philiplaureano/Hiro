using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents a service implementation that emits a property setter call.
    /// </summary>
    public class PropertyImplementation : Implementation<PropertyInfo>
    {
        /// <summary>
        /// Initializes a new instance of the PropertyImplementation class.
        /// </summary>
        /// <param name="property">The target property.</param>
        /// <param name="resolver">The dependency resolver.</param>
        public PropertyImplementation(PropertyInfo property, IDependencyResolver<PropertyInfo> resolver)
            : base(property, resolver)
        {
        }        
    }
}
