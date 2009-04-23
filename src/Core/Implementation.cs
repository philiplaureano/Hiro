using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;

namespace Hiro
{
    // Represents a class that can implement a particular dependency.
    public abstract class Implementation<TMember> : IImplementation<TMember>
    {
        private TMember _member;
        private IDependencyResolver<TMember> _resolver;

        /// <summary>
        /// Initializes a new instance of the Implementation class.
        /// </summary>
        /// <param name="member">The target member.</param>
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

        /// <summary>
        /// Emits the instructions necessary to instantiate the target service.
        /// </summary>
        /// <param name="context">The <see cref="IServiceEmitterContext"/> that contains the information required to emit the service instance. </param>
        public abstract void Emit(IServiceEmitterContext context);
    }
}
