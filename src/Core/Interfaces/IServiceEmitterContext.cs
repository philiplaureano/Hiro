using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a class that encapsulates the necessary information to emit a service implementation.
    /// </summary>
    public interface IServiceEmitterContext
    {
        /// <summary>
        /// Gets the value indicating the <see cref="IMicroContainer"/> implementation to be generated.
        /// </summary>
        /// <value>The container type.</value>
        TypeDefinition ContainerType { get; }

        /// <summary>
        /// Gets the value indicating the module that contains the container type.
        /// </summary>
        /// <value>The host module definition.</value>
        ModuleDefinition HostModule { get; }

        /// <summary>
        /// Gets the value indicating the method that will be used to instantiate the service instances.
        /// </summary>
        /// <value>The factory method.</value>
        MethodDefinition FactoryMethod { get; }

        /// <summary>
        /// Gets the value indicating the <see cref="IDependency"/> instance that will be used to describe the service that is currently being emitted.
        /// </summary>
        /// <value>The target dependency.</value>
        IDependency TargetDependency { get; }

        /// <summary>
        /// Gets the value indicating the implementation that will be used to implement the target dependency.
        /// </summary>
        /// <value>The service implementation.</value>
        IImplementation TargetImplementation { get; }

        /// <summary>
        /// Gets the value indicating the <see cref="IDependencyContainer"/> that will be used to provide the service dependencies.
        /// </summary>
        /// <value>The dependency container.</value>
        IDependencyContainer DependencyContainer { get; }
    }
}
