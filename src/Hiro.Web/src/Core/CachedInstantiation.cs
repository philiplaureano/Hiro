using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Interfaces;
using Mono.Cecil;
using Hiro.Containers;
using Mono.Cecil.Cil;

namespace Hiro.Web
{
    /// <summary>
    /// Represents a class that emits a cached service instance.
    /// </summary>
    public class CachedInstantiation : IImplementation
    {
        private readonly IStaticImplementation _actualImplementation;

        /// <summary>
        /// Initializes a new instances of the <see cref="CachedInstantiation"/> class.
        /// </summary>
        /// <param name="actualImplementation"></param>
        public CachedInstantiation(IStaticImplementation actualImplementation)
        {
            _actualImplementation = actualImplementation;
        }

        /// <summary>
        /// Emits the instructions that will instantiate the cached service implementation.
        /// </summary>
        /// <param name="dependency">The dependency that describes the service to be instantiated.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="targetMethod">The target method.</param>
        public void Emit(IDependency dependency, IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition targetMethod)
        {
            var declaringType = targetMethod.DeclaringType;
            var module = declaringType.Module;

            // Use the ICache instance that resides within the current container
            var cacheType = module.ImportType<ICache>();            
            var methodBody = targetMethod.Body;

            var locals = methodBody.Variables;

            var cacheVariable = new VariableDefinition(cacheType);
            locals.Add(cacheVariable);

            var worker = methodBody.GetILProcessor();            
            
            // var cache = (ICache)container.GetInstance(cacheType, string.Empty);
            var getCacheMethod = typeof(CacheRegistry).GetMethod("GetCache");
            var getCacheInstanceMethod = module.Import(getCacheMethod);

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Call, getCacheInstanceMethod);
            worker.Emit(OpCodes.Stloc, cacheVariable);

            worker.Emit(OpCodes.Ldloc, cacheVariable);

            var createInstance = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Brfalse, createInstance);
            
            // if (cache != null) {
            var getItemMethod = module.ImportMethod<ICache>("get_Item");

            var cacheKey = string.Format("Hiro Service Instance (ServiceName: {0}, ServiceType: {1}, Hash: {2})",
                dependency.ServiceName,
                dependency.ServiceType,
                dependency.GetHashCode());

            // cachedResult = cache[cacheKey];
            worker.Emit(OpCodes.Ldloc, cacheVariable);
            worker.Emit(OpCodes.Ldstr, cacheKey);
            worker.Emit(OpCodes.Callvirt, getItemMethod);
            worker.Emit(OpCodes.Brfalse, createInstance);

            // if (cachedResult != null) 
            //    return cachedResult;
            var endLabel = worker.Create(OpCodes.Nop);

            var objectType = module.ImportType(typeof(object));
            var resultVariable = new VariableDefinition(objectType);
            methodBody.Variables.Add(resultVariable);

            worker.Emit(OpCodes.Ldloc, cacheVariable);
            worker.Emit(OpCodes.Ldstr, cacheKey);
            worker.Emit(OpCodes.Callvirt, getItemMethod);
            worker.Emit(OpCodes.Stloc, resultVariable);
            worker.Emit(OpCodes.Br, endLabel);

            worker.Append(createInstance);            

            // Instantiate the actual service instance
            _actualImplementation.Emit(dependency, serviceMap, targetMethod);
            worker.Emit(OpCodes.Stloc, resultVariable);

            // Cache the results
            worker.Emit(OpCodes.Ldloc, cacheVariable);
            worker.Emit(OpCodes.Brfalse, endLabel);

            var setItemMethod = module.ImportMethod<ICache>("set_Item");

            // cache[cacheKey] = result;
            worker.Emit(OpCodes.Ldloc, cacheVariable);
            worker.Emit(OpCodes.Ldstr, cacheKey);
            worker.Emit(OpCodes.Ldloc, resultVariable);            
            worker.Emit(OpCodes.Callvirt, setItemMethod);

            worker.Append(endLabel);
            worker.Emit(OpCodes.Ldloc, resultVariable);
        }

        /// <summary>
        /// Gets the list of missing dependencies from the current implementation.
        /// </summary>
        /// <param name="map">The implementation map.</param>
        /// <returns>A list of missing dependencies.</returns>
        public IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer map)
        {
            return _actualImplementation.GetMissingDependencies(map);
        }

        /// <summary>
        /// Returns the dependencies required by the current implementation.
        /// </summary>
        /// <returns>The list of required dependencies required by the current implementation.</returns>
        public IEnumerable<IDependency> GetRequiredDependencies(IDependencyContainer map)
        {
            return _actualImplementation.GetRequiredDependencies(map);
        }

        /// <summary>
        /// Gets the value indicating the type that will be instantiated by this implementation.
        /// </summary>
        /// <value>The target type.</value>
        public Type TargetType
        {
            get 
            {
                return _actualImplementation.TargetType;
            }
        }
    }
}
