using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Containers;
using Hiro.Implementations;
using Hiro.Interfaces;

namespace Hiro.Resolvers
{
    /// <summary>
    /// Represents a class that selects the constructor with the most resolvable parameters.
    /// </summary>
    public class ConstructorResolver : IConstructorResolver
    {
        /// <summary>
        /// Determines which constructor implementation should be used from a given <see cref="IDependencyContainer"/> instance.
        /// </summary>
        /// <param name="targetType">The target type that contains list of constructors to be resolved.</param>
        /// <param name="container">The dependency container that holds the current set of dependencies.</param>
        /// <returns>An implementation that can instantiate the object associated with the constructor.</returns>
        public virtual IImplementation<ConstructorInfo> ResolveFrom(Type targetType, IDependencyContainer container)
        {
            IImplementation<ConstructorInfo> result = null;

            var constructors = new List<IImplementation<ConstructorInfo>>();
            foreach(var constructor in targetType.GetConstructors())
            {
                var constructorCall = CreateConstructorCall(constructor);
                constructors.Add(constructorCall);
            }

            var bestParameterCount = 0;
            var index=-1;
            var missingIndex =0;
            foreach (var constructor in constructors)
            {
            	index++;
                var missingDependencies = constructor.GetMissingDependencies(container);
                var missingItems = new List<IDependency>(missingDependencies);
                var hasMissingDependencies = missingDependencies == null || missingItems.Count > 0;                
                if (hasMissingDependencies)
                {
                	missingIndex = index;
                	continue;
                }

                var targetConstructor = constructor.Target;
                var parameters = targetConstructor.GetParameters();
                var parameterCount = parameters.Length;

                if (result == null || parameterCount > bestParameterCount)
                {
                    result = constructor;
                    bestParameterCount = parameterCount;
                }
            }
            if(constructors.Count==0) return null;
            return result??constructors[missingIndex];
        }

        /// <summary>
        /// Creates the <see cref="IImplementation"/> instance that will generate the given constructor call.
        /// </summary>
        /// <param name="constructor">The target constructor.</param>
        /// <returns>The target implementation.</returns>
        protected virtual IImplementation<ConstructorInfo> CreateConstructorCall(ConstructorInfo constructor)
        {
            return new ConstructorCall(constructor);
        }
    }
}
