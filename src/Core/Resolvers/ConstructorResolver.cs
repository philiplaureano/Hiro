using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Resolvers
{
    /// <summary>
    /// Represents a class that selects the constructor with the most resolvable parameters.
    /// </summary>
    public class ConstructorResolver
    {
        /// <summary>
        /// The list of available constructors.
        /// </summary>
        private IEnumerable<IImplementation<ConstructorInfo>> _constructors;

        /// <summary>
        /// Initializes a new instance of the ConstructorResolver class.
        /// </summary>
        /// <param name="constructors">The list of constructors.</param>
        public ConstructorResolver(IEnumerable<IImplementation<ConstructorInfo>> constructors)
        {
            _constructors = constructors;
        }

        /// <summary>
        /// Determines which constructor implementation should be used from a given <see cref="IDependencyContainer"/> instance.
        /// </summary>
        /// <param name="container">The dependency container that holds the current set of dependencies.</param>
        /// <returns>An implementation that can instantiate the object associated with the constructor.</returns>
        public virtual IImplementation<ConstructorInfo> ResolveFrom(IDependencyContainer container)
        {
            IImplementation<ConstructorInfo> result = null;

            var bestParameterCount = 0;
            foreach (var constructor in _constructors)
            {
                var missingDependencies = constructor.GetMissingDependencies(container);
                var missingItems = new List<IDependency>(missingDependencies);
                var hasMissingDependencies = missingDependencies == null || missingItems.Count > 0;
                if (hasMissingDependencies)
                    continue;

                ChooseConstructor(ref result, ref bestParameterCount, constructor);
            }

            return result;
        }

        /// <summary>
        /// Selects the constructor with the most resolvable parameters.
        /// </summary>
        /// <param name="result">The variable that will store the best match.</param>
        /// <param name="bestParameterCount">The parameter count of the current best matching constructor.</param>
        /// <param name="constructor">The current constructor.</param>
        private static void ChooseConstructor(ref IImplementation<ConstructorInfo> result, ref int bestParameterCount, IImplementation<ConstructorInfo> constructor)
        {
            var targetConstructor = constructor.Target;
            var parameters = targetConstructor.GetParameters();
            var parameterCount = parameters.Length;

            if (result != null && parameterCount <= bestParameterCount)
                return;

            result = constructor;
            bestParameterCount = parameterCount;
        }
    }
}
