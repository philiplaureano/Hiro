using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hiro
{
    // Represents a class that can implement a particular dependency.
    public class Implementation<TMember>
        where TMember : MemberInfo
    {
        private TMember _member;

        /// <summary>
        /// Initializes a new instance of the Implementation class.
        /// </summary>
        /// <param name="member">The target member.</param>
        public Implementation(TMember member)
        {
            _member = member;
        }

        /// <summary>
        /// Gets the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="resolver">The dependency resolver.</param>
        /// <returns>A list of missing dependencies.</returns>
        public IEnumerable<IDependency> GetMissingDependencies(IDependencyMap map, IDependencyResolver<TMember> resolver)
        {
            foreach (var dependency in resolver.GetDependenciesFrom(_member))
            {
                if (!map.Contains(dependency))
                    yield return dependency;
            }
        }
    }
}
