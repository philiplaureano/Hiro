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
    public class ConstructorCall : IImplementation<ConstructorInfo>
    {
        /// <summary>
        /// Initializes a new instance of the ConstructorCall class.
        /// </summary>
        /// <param name="constructor">The target constructor.</param>
        public ConstructorCall(ConstructorInfo constructor)
        {
            Target = constructor;
        }

        /// <summary>
        /// Gets the value indicating the type that will be instantiated by this implementation.
        /// </summary>
        /// <value>The target type.</value>
        public Type TargetType
        {
            get
            {
                return Target.DeclaringType;
            }
        }

        /// <summary>
        /// Gets the value indicating the target member.
        /// </summary>
        /// <value>The target member.</value>
        public ConstructorInfo Target
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>A list of missing dependencies.</returns>
        public IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer map)
        {
            foreach (var dependency in GetRequiredDependencies())
            {
                if (!map.Contains(dependency))
                    yield return dependency;
            }
        }

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public IEnumerable<IDependency> GetRequiredDependencies()
        {
            foreach (var parameter in Target.GetParameters())
            {
                var dependency = GetDependency(parameter);
                yield return dependency;
            }
        }

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="targetMethod">The target method.</param>
        public void Emit(IDependency dependency, IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition targetMethod)
        {
            var declaringType = targetMethod.DeclaringType;
            var module = declaringType.Module;
            var body = targetMethod.Body;
            var worker = body.CilWorker;

            // Instantiate the parameter values            
            foreach (var currentDependency in GetRequiredDependencies())
            {
                var implementation = serviceMap[currentDependency];
                implementation.Emit(currentDependency, serviceMap, targetMethod);
            }

            var targetConstructor = module.Import(Target);
            worker.Emit(OpCodes.Newobj, targetConstructor);
        }       

        /// <summary>
        /// Determines which dependency should be used for the target parameter.
        /// </summary>
        /// <param name="parameter">The constructor parameter.</param>
        /// <returns>A <see cref="IDependency"/> instance that represents the dependency that will be used for the target parameter.</returns>
        protected virtual IDependency GetDependency(ParameterInfo parameter)
        {
            return new Dependency(parameter.ParameterType, string.Empty);
        }
    }
}
