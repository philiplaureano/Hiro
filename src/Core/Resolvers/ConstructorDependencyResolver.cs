using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hiro.Resolvers
{
    /// <summary>
    /// Represents a class that resolves the dependencies from a target constructor.
    /// </summary>
    public class ConstructorDependencyResolver : IDependencyResolver<ConstructorInfo>
    {
        /// <summary>
        /// Returns a list of parameter dependencies from a target constructor.
        /// </summary>
        /// <param name="constructor">The target constructor.</param>
        /// <returns>The list of <see cref="IDependency"/> instances that describe the constructor parameter dependencies.</returns>
        public IEnumerable<IDependency> GetDependenciesFrom(ConstructorInfo constructor)
        {
            foreach (var param in constructor.GetParameters())
            {
                yield return new Dependency(string.Empty, param.ParameterType);
            }
        }
    }
}
