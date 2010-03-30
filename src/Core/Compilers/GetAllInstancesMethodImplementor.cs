using System;
using System.Collections.Generic;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NGenerics.DataStructures.General;
using Hiro.Containers;

namespace Hiro.Compilers
{
    internal class GetAllInstancesMethodImplementor : IGetAllInstancesMethodImplementor
    {
        private readonly IServiceInitializer _initializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllInstancesMethodImplementor"/> class.
        /// </summary>
        public GetAllInstancesMethodImplementor() : this(new ServiceInitializer())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAllInstancesMethodImplementor"/> class.
        /// </summary>
        /// <param name="initializer">The <see cref="IServiceInitializer"/> instance that will initialize the service types with the current container.</param>
        public GetAllInstancesMethodImplementor(IServiceInitializer initializer)
        {
            _initializer = initializer;
        }

        public void DefineGetAllInstancesMethod(TypeDefinition containerType, ModuleDefinition module, 
            IDictionary<IDependency, IImplementation> serviceMap)
        {
            var targetMethods = new List<MethodDefinition>();
            foreach (MethodDefinition method in containerType.Methods)
            {
                if (method.Name != "GetAllInstances")
                    continue;

                targetMethods.Add(method);
            }

            var getAllInstancesMethod = targetMethods[0];

            // Remove the stub implementation
            var body = getAllInstancesMethod.Body;
            var worker = body.CilWorker;

            body.InitLocals = true;
            body.Instructions.Clear();

            var listVariable = getAllInstancesMethod.AddLocal<List<object>>();
            var listCtor = module.ImportConstructor<List<object>>();
            worker.Emit(OpCodes.Newobj, listCtor);
            worker.Emit(OpCodes.Stloc, listVariable);

            // Group the dependencies by type
            var dependenciesByType = new HashList<Type, IDependency>();
            foreach (var dependency in serviceMap.Keys)
            {
                var serviceType = dependency.ServiceType;
                dependenciesByType.Add(serviceType, dependency);
            }

            var getTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var getTypeFromHandle = module.Import(getTypeFromHandleMethod);
            var addItem = module.ImportMethod<List<object>>("Add");

            var currentService = getAllInstancesMethod.AddLocal<object>();

            foreach (var currentType in dependenciesByType.Keys)
            {
                var currentTypeRef = module.Import(currentType);

                var currentList = dependenciesByType[currentType];
                if (currentList.Count == 0)
                    continue;

                var skipAdd = worker.Create(OpCodes.Nop);
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldtoken, currentTypeRef);
                worker.Emit(OpCodes.Call, getTypeFromHandle);
                worker.Emit(OpCodes.Ceq);
                worker.Emit(OpCodes.Brfalse, skipAdd);

                foreach (var dependency in currentList)
                {
                    worker.Emit(OpCodes.Ldloc, listVariable);
                    var implementation = serviceMap[dependency];
                    implementation.Emit(dependency, serviceMap, getAllInstancesMethod);
                    worker.Emit(OpCodes.Stloc, currentService);

                    // Call IInitialize.Initialize(container) on the current service type
                    _initializer.Initialize(worker, module, currentService);

                    worker.Emit(OpCodes.Ldloc, currentService);
                    worker.Emit(OpCodes.Callvirt, addItem);
                }

                worker.Append(skipAdd);
            }

            var skipOtherContainerCall = worker.Create(OpCodes.Nop);

            var getNextContainer = module.ImportMethod<IMicroContainer>("get_NextContainer");
            var otherContainer = getAllInstancesMethod.AddLocal<IMicroContainer>();

            // var otherContainer = this.NextContainer;
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Callvirt, getNextContainer);
            worker.Emit(OpCodes.Stloc, otherContainer);

            // if (otherContainer != null && this != otherContainer) {
            worker.Emit(OpCodes.Ldloc, otherContainer);
            worker.Emit(OpCodes.Brfalse, skipOtherContainerCall);

            worker.Emit(OpCodes.Ldloc, otherContainer);
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ceq);
            worker.Emit(OpCodes.Brtrue, skipOtherContainerCall);

            // var otherInstances = NextContainer.GetAllInstances(type);
            var otherGetAllInstancesMethod = module.ImportMethod<IMicroContainer>("GetAllInstances");

            worker.Emit(OpCodes.Ldloc, listVariable);
            worker.Emit(OpCodes.Ldloc, otherContainer);            
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Callvirt, otherGetAllInstancesMethod);

            // resultList.AddRange(otherInstances);
            var addRangeMethod = module.ImportMethod<List<object>>("AddRange");
            worker.Emit(OpCodes.Callvirt, addRangeMethod);

            // }

            // Cast the results down to an IEnumerable<object>
            var enumerableType = module.ImportType<IEnumerable<object>>();
            worker.Append(skipOtherContainerCall);
            worker.Emit(OpCodes.Ldloc, listVariable);
            worker.Emit(OpCodes.Isinst, enumerableType);                        
            worker.Emit(OpCodes.Ret);
        }
    }
}
