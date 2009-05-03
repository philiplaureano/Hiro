﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents an <see cref="IImplementation"/> type that adds property injection capabilities to other <see cref="IImplementation"/> instances.
    /// </summary>
    public class PropertyInjector : IImplementation
    {
        /// <summary>
        /// The implementation that will instantiate the target type.
        /// </summary>
        private IImplementation _implementation;

        /// <summary>
        /// The functor that determines which properties will be injected.
        /// </summary>
        private Func<PropertyInfo, bool> _propertyFilter;

        /// <summary>
        /// The functor that determines the dependencies that will be injected into each property.
        /// </summary>
        private Func<PropertyInfo, IDependency> _propertyDependencyResolver;

        /// <summary>
        /// Initializes a new instance of the PropertyInjector class.
        /// </summary>
        /// <param name="implementation">The target implementation that will instantiate the service type.</param>
        public PropertyInjector(IImplementation implementation)
            : this(implementation, p => p.CanWrite, p => new Dependency(p.PropertyType))
        {
        }

        /// <summary>
        /// Initializes a new instance of the PropertyInjector class.
        /// </summary>
        /// <param name="implementation">The target implementation that will instantiate the service type.</param>
        /// <param name="propertyFilter">The functor that determines which properties will be injected.</param>
        /// <param name="propertyDependencyResolver">The functor that determines the dependencies that will be injected into each property.</param>
        public PropertyInjector(IImplementation implementation, Func<PropertyInfo, bool> propertyFilter, Func<PropertyInfo, IDependency> propertyDependencyResolver)
        {
            _implementation = implementation;
            _propertyFilter = propertyFilter;
            _propertyDependencyResolver = propertyDependencyResolver;
        }

        /// <summary>
        /// Gets the value indicating the type that will be instantiated by this implementation.
        /// </summary>
        /// <value>The target type.</value>
        public Type TargetType
        {
            get
            {
                return _implementation.TargetType;
            }
        }

        /// <summary>
        /// Gets the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>A list of missing dependencies.</returns>
        public IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer map)
        {
            var missingDependencies = GetRequiredDependencies().Where(d => !map.Contains(d));

            return missingDependencies;
        }

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public IEnumerable<IDependency> GetRequiredDependencies()
        {
            var requiredDependencies = _implementation.GetRequiredDependencies();
            var results = new HashSet<IDependency>(requiredDependencies);

            var properties = TargetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                // Skip properties that don't meet the filter criteria
                if (!_propertyFilter(property) || !property.CanWrite)
                    continue;

                var dependency = _propertyDependencyResolver(property);
                if (dependency == null || results.Contains(dependency))
                    continue;

                results.Add(dependency);
            }

            return results;
        }

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="targetMethod">The target method.</param>
        public void Emit(IDependency dependency, IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition targetMethod)
        {
            var declaringType = targetMethod.DeclaringType;
            var module = declaringType.Module;
            var body = targetMethod.Body;
            var worker = body.CilWorker;

            // Emit the target implementation
            _implementation.Emit(dependency, serviceMap, targetMethod);

            // Determine the properties that need injection
            var targetProperties = from p in TargetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                   where _propertyFilter(p) && _propertyDependencyResolver(p) != null && p.CanWrite
                                   select p;

            foreach (var property in targetProperties)
            {
                var curentDependency = _propertyDependencyResolver(property);
                if (!serviceMap.ContainsKey(curentDependency))
                    continue;

                // Push the target onto the stack
                worker.Emit(OpCodes.Dup);

                // Get the code that will instantiate the property value
                var propertyValueImplementation = serviceMap[curentDependency];
                propertyValueImplementation.Emit(curentDependency, serviceMap, targetMethod);

                // Call the setter
                var setterMethod = property.GetSetMethod();
                var setter = module.Import(setterMethod);

                var callInstruction = setterMethod.IsVirtual ? worker.Create(OpCodes.Callvirt, setter) : worker.Create(OpCodes.Call, setter);
                worker.Append(callInstruction);
            }
        }
    }
}
