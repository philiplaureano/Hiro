using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using NGenerics.DataStructures.General;

namespace Hiro
{
    /// <summary>
    /// Represents a class that can map dependencies to implementations.
    /// </summary>
    public abstract class BaseDependencyMap : IDependencyMap
    {
        /// <summary>
        /// The list of dependencies in the current map.
        /// </summary>
        protected HashList<IDependency, IImplementation> _entries = new HashList<IDependency, IImplementation>();        

        /// <summary>
        /// Gets or sets the value indicating the <see cref="IImplementationInjector"/> instance that will be used to intercept <see cref="IImplementation"/> instances.
        /// </summary>
        /// <value>The implementation injector.</value>
        public IImplementationInjector Injector { get; set; }

        /// <summary>
        /// Gets the value indicating the list of dependencies that currently exist within the current container.
        /// </summary>
        /// <value>The current list of dependencies.</value>
        public IEnumerable<IDependency> Dependencies
        {
            get
            {
                return _entries.Keys;
            }
        }

        /// <summary>
        /// Determines whether or not a particular service dependency exists in the current dependency container.
        /// </summary>
        /// <param name="dependency">The target service dependency.</param>
        /// <returns><c>true</c> if the service exists; otherwise, it will return <c>false</c>.</returns>
        public bool Contains(IDependency dependency)
        {
            return _entries.ContainsKey(dependency);
        }

        /// <summary>
        /// Associates the given <paramref name="implementation"/> with the target <paramref name="dependency"/>.
        /// </summary>
        /// <param name="dependency">The dependency that will be associated with the implementation.</param>
        /// <param name="implementation">The implementation itself.</param>
        public void AddService(IDependency dependency, IImplementation implementation)
        {
            var currentImplementation = implementation;

            if (Injector != null)
                currentImplementation = Injector.Inject(dependency, currentImplementation);

            _entries.Add(dependency, currentImplementation);
        }
       
        /// <summary>
        /// Gets the current list of implementations for the current dependency.
        /// </summary>
        /// <param name="targetDependency">The target dependency.</param>
        /// <param name="addIncompleteImplementations">A boolean flag that determines whether or not the resulting list should include implementations with incomplete dependencies.</param>
        /// <returns>A list of implementations.</returns>
        public IEnumerable<IImplementation> GetImplementations(IDependency targetDependency, bool addIncompleteImplementations)
        {
            if (!_entries.ContainsKey(targetDependency))
                yield break;

            var items = _entries[targetDependency];
            foreach (var item in items)
            {
                var missingDependencies = item.GetMissingDependencies(this);
                var missingItems = new List<IDependency>(missingDependencies);
                var completed = missingItems.Count == 0;

                if (completed || (!completed && addIncompleteImplementations))
                    yield return item;
            }
        }

        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="other">The System.Object to compare with the current System.Object.</param>
        /// <returns>true if the specified System.Object is equal to the current System.Object; otherwise, false.</returns>
        public override bool Equals(object other)
        {
            return other.GetHashCode() == GetHashCode();
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current System.Object.</returns>
        public override int GetHashCode()
        {
            int hash = 0;

            foreach (var entry in _entries)
            {
                var dependency = entry.Key;
                hash ^= dependency.GetHashCode();
            }

            return hash;
        }
    }
}
