using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hiro
{
    // Represents a class that can implement a particular dependency.
    public class Implementation<TMember> : IImplementation
    {
        private TMember _member;
        private IDependencyResolver<TMember> _resolver;

        /// <summary>
        /// Initializes a new instance of the Implementation class.
        /// </summary>
        /// <param name="member">The target member.</param>
        public Implementation(TMember member, IDependencyResolver<TMember> resolver)
        {
            _member = member;
            _resolver = resolver;
        }        

        /// <summary>
        /// Obtains the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The dependency map.</param>
        /// <param name="resolver">The dependency resolver.</param>
        /// <returns>A list of missing dependencies.</returns>
        public virtual IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer map)
        {
            foreach (var dependency in _resolver.GetDependenciesFrom(_member))
            {
                if (!map.Contains(dependency))
                    yield return dependency;
            }
        }

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public virtual IEnumerable<IDependency> GetRequiredDependencies()
        {
            return _resolver.GetDependenciesFrom(_member);
        }
    }
}
