using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Iesi.Collections.Generic;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents an <see cref="IImplementation"/> type that adds property injection capabilities to other <see cref="IImplementation"/> instances.
    /// </summary>
    public class PropertyInjectionCall : IImplementation
    {
        /// <summary>
        /// The implementation that will instantiate the target type.
        /// </summary>
        private readonly IStaticImplementation _implementation;

        /// <summary>
        /// The functor that determines which properties will be injected.
        /// </summary>
        private readonly Func<PropertyInfo, bool> _propertyFilter;

        /// <summary>
        /// The functor that determines the dependencies that will be injected into each property.
        /// </summary>
        private readonly Func<PropertyInfo, IDependency> _propertyDependencyResolver;

        /// <summary>
        /// Initializes a new instance of the PropertyInjector class.
        /// </summary>
        /// <param name="implementation">The target implementation that will instantiate the service type.</param>
        public PropertyInjectionCall(IStaticImplementation implementation)
            : this(implementation, p => p.CanWrite, p => new Dependency(p.PropertyType))
        {
        }

        /// <summary>
        /// Initializes a new instance of the PropertyInjector class.
        /// </summary>
        /// <param name="implementation">The target implementation that will instantiate the service type.</param>
        /// <param name="propertyFilter">The functor that determines which properties will be injected.</param>
        /// <param name="propertyDependencyResolver">The functor that determines the dependencies that will be injected into each property.</param>
        public PropertyInjectionCall(IStaticImplementation implementation, Func<PropertyInfo, bool> propertyFilter, Func<PropertyInfo, IDependency> propertyDependencyResolver)
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
            foreach (var dependency in GetRequiredDependencies())
            {
                if (!map.Contains(dependency))
                    yield return dependency;
            }
        }

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public IEnumerable<IDependency> GetRequiredDependencies()
        {
            var requiredDependencies = _implementation.GetRequiredDependencies();
            var dependencyList = new List<IDependency>(requiredDependencies);
            var results = new HashedSet<IDependency>(dependencyList);

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
            Func<PropertyInfo, bool> propertyFilter = p => _propertyFilter(p) && _propertyDependencyResolver(p) != null && p.CanWrite;
            var targetProperties = TargetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in targetProperties)
            {
                if (!propertyFilter(property))
                    continue;

                var curentDependency = _propertyDependencyResolver(property);
                if (!serviceMap.ContainsKey(curentDependency))
                    continue;

                EmitPropertySetter(serviceMap, targetMethod, module, worker, property, curentDependency);
            }
        }

        /// <summary>
        /// Emits the instructions that will instantiate each property value and assign it to the target property.
        /// </summary>
        /// <param name="serviceMap">The service map that contains the application dependencies.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="module">The module that hosts the container type.</param>
        /// <param name="worker">The <see cref="CilWorker"/> that points to the target method body.</param>
        /// <param name="property">The target property.</param>
        /// <param name="curentDependency">The <see cref="IDependency"/> that describes the service instance that will be assigned to the target property.</param>
        private static void EmitPropertySetter(IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition targetMethod, ModuleDefinition module, CilWorker worker, PropertyInfo property, IDependency curentDependency)
        {
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
