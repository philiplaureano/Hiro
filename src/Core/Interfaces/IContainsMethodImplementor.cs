using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Mono.Cecil;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a class that implements the <see cref="IMicroContainer.Contains"/> method.
    /// </summary>
    public interface IContainsMethodImplementor
    {
        /// <summary>
        /// Emits the body of the <see cref="IMicroContainer.Contains"/> method implementation.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <param name="module">The target module.</param>
        /// <param name="getServiceHash">The method that will be used to determine the hash code of the current service.</param>
        /// <param name="jumpTargetField">The field that contains the list of jump entries.</param>
        void DefineContainsMethod(TypeDefinition containerType, ModuleDefinition module, MethodDefinition getServiceHash, FieldDefinition jumpTargetField);
    }
}
