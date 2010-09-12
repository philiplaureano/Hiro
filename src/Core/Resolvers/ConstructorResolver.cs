using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;

namespace Hiro.Resolvers
{   
    /// <summary>
    /// Represents a class that selects the constructor with the most resolvable parameters.
    /// </summary>
    public abstract class ConstructorResolver<TMethodBuilder> : IConstructorResolver<TMethodBuilder>
    {
        /// <summary>
        /// Determines which constructor implementation should be used from a given <see cref="IDependencyContainer"/> instance.
        /// </summary>
        /// <param name="targetType">The target type that contains list of constructors to be resolved.</param>
        /// <param name="container">The dependency container that holds the current set of dependencies.</param>
        /// <returns>An implementation that can instantiate the object associated with the constructor.</returns>
        public virtual IStaticImplementation<ConstructorInfo, TMethodBuilder> ResolveFrom(Type targetType, IDependencyContainer<TMethodBuilder> container)
        {
            IStaticImplementation<ConstructorInfo, TMethodBuilder> result = null;

            var constructors = new List<IStaticImplementation<ConstructorInfo, TMethodBuilder>>();
            foreach(var constructor in targetType.GetConstructors())
            {
                var constructorCall = CreateConstructorCall(constructor);
                constructors.Add(constructorCall);
            }

            var bestParameterCount = 0;
            foreach (var constructor in constructors)
            {
                var missingDependencies = constructor.GetMissingDependencies(container);
                var missingItems = new List<IDependency>(missingDependencies);
                var hasMissingDependencies = missingDependencies == null || missingItems.Count > 0;
                if (hasMissingDependencies)
                    continue;

                var targetConstructor = constructor.Target;
                var parameters = targetConstructor.GetParameters();
                var parameterCount = parameters.Length;

                if (result == null || parameterCount > bestParameterCount)
                {
                    result = constructor;
                    bestParameterCount = parameterCount;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the <see cref="IImplementation"/> instance that will generate the given constructor call.
        /// </summary>
        /// <param name="constructor">The target constructor.</param>
        /// <returns>The target implementation.</returns>
        protected abstract IStaticImplementation<ConstructorInfo, TMethodBuilder> CreateConstructorCall(
            ConstructorInfo constructor);
    }
}
