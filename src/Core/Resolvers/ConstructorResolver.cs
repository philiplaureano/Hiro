using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Hiro.Interfaces;

namespace Hiro.Resolvers
{
    public class ConstructorResolver
    {
        private IEnumerable<IImplementation<ConstructorInfo>> _constructors;
        public ConstructorResolver(IEnumerable<IImplementation<ConstructorInfo>> constructors)
        {
            _constructors = constructors;
        }

        public IImplementation<ConstructorInfo> ResolveFrom(IDependencyContainer container)
        {
            IImplementation<ConstructorInfo> result = null;

            var bestParameterCount = 0;
            foreach (var constructor in _constructors)
            {
                var missingDependencies = constructor.GetMissingDependencies(container);
                var hasMissingDependencies = missingDependencies == null || missingDependencies.Count() > 0;
                if (hasMissingDependencies)
                    continue;

                var targetConstructor = constructor.Target;
                var parameters = targetConstructor.GetParameters();
                var parameterCount = parameters.Count();

                if (result == null)
                {
                    result = constructor;
                    bestParameterCount = parameterCount;
                    continue;
                }

                if (parameterCount <= bestParameterCount)
                    continue;

                result = constructor;
                bestParameterCount = parameterCount;
            }

            return result;
        }
    }
}
