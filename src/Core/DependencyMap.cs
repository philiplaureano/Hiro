using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGenerics.DataStructures.General;
using Hiro.Interfaces;

namespace Hiro
{
    /// <summary>
    /// Represents a class that can map dependencies to implementations.
    /// </summary>
    public class DependencyMap : IDependencyContainer
    {
        private HashList<IDependency, IImplementation> _entries = new HashList<IDependency, IImplementation>();

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
        public void AddImplementation(IDependency dependency, IImplementation implementation)
        {
            _entries.Add(dependency, implementation);
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
                var isComplete = missingDependencies.Count() == 0;

                if (isComplete || (!isComplete && addIncompleteImplementations))
                    yield return item;
            }
        }
    }
}
