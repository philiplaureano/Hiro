using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Mono.Cecil;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that can implement the <see cref="IMicroContainer.GetInstance"/> method.
    /// </summary>
    public interface IGetInstanceMethodImplementor
    {
        /// <summary>
        /// Defines the <see cref="IMicroContainer.GetInstance"/> method implementation for the container type.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <param name="module">The target module.</param>
        /// <param name="getServiceHash">The GetServiceHash method.</param>
        /// <param name="jumpTargetField">The field that will store the jump target indexes.</param>
        /// <param name="serviceMap">The service map that contains the list of existing services.</param>
        void DefineGetInstanceMethod(TypeDefinition containerType, ModuleDefinition module, MethodDefinition getServiceHash, FieldDefinition jumpTargetField, IDictionary<IDependency, IImplementation> serviceMap);
    }
}
