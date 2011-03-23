using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Functors.Core
{
    /// <summary>
    /// Represents a service implementation that uses a functor to instantiate the service itself.
    /// </summary>
    public class FunctorCall : IImplementation
    {
        private readonly string _functorId = Guid.NewGuid().ToString();
        private readonly Func<IMicroContainer, object> _functor;
        private readonly Type _serviceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctorCall"/> class.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="functor">The functor that will be used to instantiate the given service instance.</param>
        public FunctorCall(Type serviceType, Func<IMicroContainer, object> functor)
        {
            _serviceType = serviceType;
            _functor = functor;
        }

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation using the given factory functor.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="targetMethod">The target method.</param>
        public void Emit(IDependency dependency,
            IDictionary<IDependency, IImplementation> serviceMap,
            MethodDefinition targetMethod)
        {
            var declaringType = targetMethod.DeclaringType;
            var module = declaringType.Module;

            var serviceType = module.Import(_serviceType);
            var createInstanceMethod = typeof(FunctorRegistry).GetMethod("CreateInstance");
            var createInstance = module.Import(createInstanceMethod);

            // Register the functor 
            FunctorRegistry.AddFunctor(_functorId, _functor);

            var body = targetMethod.Body;
            var IL = body.GetILProcessor();

            // Instantiate the instance at runtime using the
            // given functor associated with the functor id
            IL.Emit(OpCodes.Ldstr, _functorId);
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Call, createInstance);

            if (serviceType.IsValueType) 
                return;

            IL.Emit(OpCodes.Castclass, serviceType);
        }

        /// <summary>
        /// Gets the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>A list of missing dependencies.</returns>
        public virtual IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer map)
        {
            yield break;
        }

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public virtual IEnumerable<IDependency> GetRequiredDependencies(IDependencyContainer map)
        {
            yield break;
        }
    }
}
