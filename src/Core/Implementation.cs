using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;

namespace Hiro
{
    /// <summary>
    /// Represents a class that can implement a particular dependency.
    /// </summary>
    /// <typeparam name="TMember">The member type.</typeparam>
    public abstract class Implementation<TMember> : IImplementation<TMember>
    {
        /// <summary>
        /// The target member.
        /// </summary>
        private TMember _member;

        /// <summary>
        /// The dependency resolver that will be used to determine which dependencies are required by this instance.
        /// </summary>
        private IDependencyResolver<TMember> _resolver;

        /// <summary>
        /// Initializes a new instance of the Implementation class.
        /// </summary>
        /// <param name="member">The target member.</param>
        /// <param name="resolver">The dependency resolver.</param>
        protected Implementation(TMember member, IDependencyResolver<TMember> resolver)
        {
            _member = member;
            _resolver = resolver;
        }

        /// <summary>
        /// Gets the value indicating the target member.
        /// </summary>
        /// <value>The target member.</value>
        public TMember Target
        {
            get { return _member; }
        }

        /// <summary>
        /// Obtains the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The dependency map.</param>
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
