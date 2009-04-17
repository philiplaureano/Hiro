using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hiro.Resolvers
{
    /// <summary>
    /// Represents a type that can infer the dependencies of a target property.
    /// </summary>
    public class PropertyDependencyResolver : IDependencyResolver<PropertyInfo>
    {
        /// <summary>
        /// Infers the dependencies of a target property.
        /// </summary>
        /// <param name="property">The target property.</param>
        /// <returns>A list of dependencies.</returns>
        public IEnumerable<IDependency> GetDependenciesFrom(PropertyInfo property)
        {
            yield return new Dependency(string.Empty, property.PropertyType);
        }
    }
}
