using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Mono.Cecil;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a class that implements the <see cref="IMicroContainer.GetAllInstances"/> method.
    /// </summary>
    public interface IGetAllInstancesMethodImplementor
    {
        /// <summary>
        /// Emits the body of the <see cref="IMicroContainer.GetAllInstances"/> method implementation.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <param name="module">The target module.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies that will be emitted into the method.</param>
        void DefineGetAllInstancesMethod(TypeDefinition containerType, ModuleDefinition module, IDictionary<IDependency, IImplementation> serviceMap);
    }
}
