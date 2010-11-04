using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents an implementation that emits a constructor call.
    /// </summary>
    public class ConstructorCall : BaseConstructorCall
    {
        /// <summary>
        /// Initializes a new instance of the ConstructorCall class.
        /// </summary>
        /// <param name="constructor">The target constructor.</param>
        public ConstructorCall(ConstructorInfo constructor) : base(constructor)
        {
        }

        /// <summary>
        /// Determines the <see cref="IImplementation"/> instance that will be used to emit the code
        /// that will execute if the <paramref name="currentDependency"/> cannot be resolved
        /// when the container is compiled.
        /// </summary>
        /// <param name="currentDependency">The unresolved dependency.</param>
        /// <returns>The target implementation that will be executed in place of the original dependency.</returns>
        protected override IImplementation GetUnresolvedDependency(IDependency currentDependency)
        {
            return new ContainerCall(currentDependency.ServiceType, currentDependency.ServiceName);
        }
    }
}
