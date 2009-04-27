using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;
using LinFu.Reflection.Emit;
using Mono.Cecil;
using Hiro.Containers;
using Mono.Cecil.Cil;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents an implementation that emits a constructor call.
    /// </summary>
    public class ConstructorImplementation : IImplementation<ConstructorInfo>
    {
        /// <summary>
        /// Initializes a new instance of the ConstructorImplementation class.
        /// </summary>
        /// <param name="constructor">The target constructor.</param>
        public ConstructorImplementation(ConstructorInfo constructor)
        {
            Target = constructor;
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
        /// <param name="targetMethod">The method that will instantiate the service itself.</param>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of available dependencies in the current application.</param>
        public void Emit(MethodDefinition targetMethod, IDependency dependency, IDictionary<IDependency, IImplementation> serviceMap)
        {
            var declaringType = targetMethod.DeclaringType;
            var module = declaringType.Module;
            var body = targetMethod.Body;
            var worker = body.CilWorker;

            // Instantiate the parameter values
            var getTypeFromHandle = module.ImportMethod<Type>("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Instance);
            var getInstanceMethod = module.ImportMethod<IMicroContainer>("GetInstance");
            foreach (var currentDependency in GetRequiredDependencies())
            {
                // Push the container instance
                worker.Emit(OpCodes.Ldarg_0);

                // Push the service type onto the stack
                var serviceType = module.Import(currentDependency.ServiceType);
                worker.Emit(OpCodes.Ldtoken, serviceType);
                worker.Emit(OpCodes.Call, getTypeFromHandle);

                // Push the service name onto the stack
                var serviceName = currentDependency.ServiceName;
                var pushName = String.IsNullOrEmpty(serviceName) ? worker.Create(OpCodes.Ldnull) : worker.Emit(OpCodes.Ldstr, serviceName);
                worker.Append(pushName);

                // var argN = (ArgumentType)this.GetInstance(serviceType, serviceName);
                worker.Emit(OpCodes.Callvirt, getInstanceMethod);
                worker.Emit(OpCodes.Unbox_Any, serviceType);
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
            return new Dependency(string.Empty, parameter.ParameterType);
        }
    }
}
