using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;
using Mono.Cecil;

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

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation.
        /// </summary>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of available dependencies in the current application.</param>
        public override void Emit(MethodDefinition targetMethod, IDependency dependency, IDictionary<IDependency, IImplementation> serviceMap)
        {
            throw new NotImplementedException();
        }
    }
}
