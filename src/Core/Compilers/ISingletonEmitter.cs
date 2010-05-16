using System.Collections.Generic;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represens a type that can instantiate singletons at runtime.
    /// </summary>
    public interface ISingletonEmitter
    {       
        /// <summary>
        /// Emits a service as a singleton type.
        /// </summary>
        /// <param name="targetMethod">The <see cref="IMicroContainer.GetInstance"/> method implementation.</param>
        /// <param name="dependency">The dependency that will be instantiated by the container.</param>
        /// <param name="implementation">The implementation that will be used to instantiate the dependency.</param>
        /// <param name="serviceMap">The service map the contains the current application dependencies.</param>
        void EmitService(MethodDefinition targetMethod,  IDependency dependency,  IImplementation implementation, 
            IDictionary<IDependency, IImplementation> serviceMap);
    }
}