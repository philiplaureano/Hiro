using System;
using System.Collections.Generic;
using System.Reflection;
using Hiro.Containers;
using Hiro.Interfaces;
using LinFu.Finders;
using LinFu.Finders.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents the basic implementation of a constructor call.
    /// </summary>
    public abstract class BaseConstructorCall : IImplementation<ConstructorInfo>
    {
        /// <summary>
        /// Initializes a new instance of the ConstructorCall class.
        /// </summary>
        /// <param name="constructor">The target constructor.</param>
        protected BaseConstructorCall(ConstructorInfo constructor)
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
            foreach (var dependency in GetRequiredDependencies(map))
            {
                if (!map.Contains(dependency))
                    yield return dependency;
            }
        }

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public virtual IEnumerable<IDependency> GetRequiredDependencies(IDependencyContainer map)
        {
            var dependencies = new List<IDependency>(map.Dependencies);
            foreach(var parameter in Target.GetParameters())
            {
                var dependency = GetNamedParameterDependencyIfPossible(dependencies, parameter);
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
            var il = body.GetILProcessor();

            var dependencies = serviceMap.Keys;

            // Instantiate the parameter values            
            foreach (var parameter in Target.GetParameters())
            {
                IDependency currentDependency = GetNamedParameterDependencyIfPossible(dependencies, parameter);

                EmitDependency(currentDependency, targetMethod, serviceMap);
            }

            var targetConstructor = module.Import(Target);
            il.Emit(OpCodes.Newobj, targetConstructor);
        }

        /// <summary>
        /// Attempts to resolve the service that matches the parameter name and parameter type if possible, and if the given service cannot be resolved,
        /// the dependency will fall back to the default parameter type.
        /// </summary>
        /// <param name="dependencies">The list of dependencies in the current container.</param>
        /// <param name="parameter">The target parameter.</param>
        /// <returns>The required dependency.</returns>
        private IDependency GetNamedParameterDependencyIfPossible(ICollection<IDependency> dependencies, ParameterInfo parameter)
        {
            var parameterType = parameter.ParameterType;
            IDependency currentDependency = new Dependency(parameterType, string.Empty);

            // Use the named parameter dependency if it exists
            var parameterName = parameter.Name;
            var dependencyList = dependencies.AsFuzzyList();
            dependencyList.AddCriteria(d => d.ServiceType == parameterType, CriteriaType.Critical);
            dependencyList.AddCriteria(d => d.ServiceName != null && d.ServiceName.ToLowerInvariant() == parameterName.ToLowerInvariant());

            var bestMatch = dependencyList.BestMatch();
            if (bestMatch != null)
                currentDependency = bestMatch.Item;

            return currentDependency;
        }

        /// <summary>
        /// Emits the necessary IL to instantiate a given service type.
        /// </summary>
        /// <param name="currentDependency">The dependency that will be instantiated.</param>
        /// <param name="targetMethod">The target method that will instantiate the service instance.</param>
        /// <param name="serviceMap">The service map that contains the target dependency to be instantiated.</param>
        private void EmitDependency(IDependency currentDependency, MethodDefinition targetMethod, IDictionary<IDependency, IImplementation> serviceMap)
        {
            IImplementation implementation = Resolve(serviceMap, currentDependency);
            implementation.Emit(currentDependency, serviceMap, targetMethod);
        }

        /// <summary>
        /// Resolves an <see cref="IImplementation"/> from the given <paramref name="currentDependency">dependency</paramref> and <paramref name="serviceMap"/>.
        /// </summary>
        /// <param name="serviceMap">The service map that contains the target dependency to be instantiated.</param>
        /// <param name="currentDependency">The dependency that will be instantiated.</param>
        /// <returns>The <see cref="IImplementation"/> instance that will be used to instantiate the dependency.</returns>
        protected virtual IImplementation Resolve(IDictionary<IDependency, IImplementation> serviceMap, IDependency currentDependency)
        {
            if (serviceMap.ContainsKey(currentDependency))
                return serviceMap[currentDependency];

            // HACK: Get the service instance at runtime if it can't be resolved at compile time
            return GetUnresolvedDependency(currentDependency);
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

        protected abstract IImplementation GetUnresolvedDependency(IDependency currentDependency);
    }
}