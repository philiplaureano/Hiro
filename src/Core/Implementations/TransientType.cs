﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;
using Hiro.Resolvers;
using Mono.Cecil;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents an implementation that can instantiate a type that has more than one constructor.
    /// </summary>
    public class TransientType : IImplementation<ConstructorInfo>
    {
        /// <summary>
        /// The type that will be instantiated by the compiled container.
        /// </summary>
        private Type _targetType;

        /// <summary>
        /// The dependency container that contains the dependencies in the given application.
        /// </summary>
        private IDependencyContainer _container;

        /// <summary>
        /// The functor that determines which constructor implementation will be used to instantiate the target type.
        /// </summary>
        private Func<IImplementation<ConstructorInfo>> _getConstructorImplementation;

        /// <summary>
        /// Initializes a new instance of the TransientType class.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <param name="container">The dependency container.</param>
        public TransientType(Type targetType, IDependencyContainer container)
        {
            _targetType = targetType;
            _container = container;

            var constructorImplementations = (from c in targetType.GetConstructors()
                                              select new ConstructorImplementation(c) as IImplementation<ConstructorInfo>).ToList();

            _getConstructorImplementation = () =>
                {
                    var resolver = new ConstructorResolver(constructorImplementations);
                    var result = resolver.ResolveFrom(_container);

                    if (result == null)
                    {
                        var message = string.Format("Unable to find a constructor for target type '{0}'", targetType.AssemblyQualifiedName);
                        throw new ConstructorNotFoundException(message);
                    }

                    return result;
                };
        }

        /// <summary>
        /// Gets the value indicating the type that will be instantiated by this implementation.
        /// </summary>
        /// <value>The target type.</value>
        public Type TargetType
        {
            get
            {
                return _targetType;
            }
        }

        /// <summary>
        /// Gets the value indicating the constructor that will be used to instantiate the implementation.
        /// </summary>
        /// <value>The target constructor.</value>
        public ConstructorInfo Target
        {
            get
            {
                return TargetImplementation.Target;
            }
        }

        /// <summary>
        /// Gets the value indicating the constructor implementation that will be used to 
        /// instantiate the target type.
        /// </summary>
        /// <value>The target implementation that will instantiate the target type.</value>
        private IImplementation<ConstructorInfo> TargetImplementation
        {
            get
            {
                return _getConstructorImplementation();
            }
        }

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <param name="map">The dependency container.</param>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer map)
        {
            return TargetImplementation.GetMissingDependencies(map);
        }

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public IEnumerable<IDependency> GetRequiredDependencies()
        {
            return TargetImplementation.GetRequiredDependencies();
        }

        /// <summary>
        /// Emits the instructions that will instantiate the current implementation.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="targetMethod">The target method.</param>
        public void Emit(IDependency dependency, IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition targetMethod)
        {
            TargetImplementation.Emit(dependency, serviceMap, targetMethod);
        }        
    }
}
