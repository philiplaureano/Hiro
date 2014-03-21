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
            var IL = body.GetILProcessor();

            body.InitLocals = true;
            body.Instructions.Clear();

            var listVariable = getAllInstancesMethod.AddLocal<List<object>>();
            var listCtor = module.ImportConstructor<List<object>>();
            IL.Emit(OpCodes.Newobj, listCtor);
            IL.Emit(OpCodes.Stloc, listVariable);

            // Group the dependencies by type
            var dependenciesByType = new HashList<System.Type, IDependency>();
            foreach (var dependency in serviceMap.Keys)
            {
                var serviceType = dependency.ServiceType;
                dependenciesByType.Add(serviceType, dependency);
            }

            var getTypeFromHandleMethod = typeof(System.Type).GetMethod("GetTypeFromHandle", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var getTypeFromHandle = module.Import(getTypeFromHandleMethod);
            var addItem = module.ImportMethod<List<object>>("Add");

            var currentService = getAllInstancesMethod.AddLocal<object>();

            foreach (var currentType in dependenciesByType.Keys)
            {
                var currentTypeRef = module.Import(currentType);

                var currentList = dependenciesByType[currentType];
                if (currentList.Count == 0)
                    continue;

                var skipAdd = IL.Create(OpCodes.Nop);
                IL.Emit(OpCodes.Ldarg_1);
                IL.Emit(OpCodes.Ldtoken, currentTypeRef);
                IL.Emit(OpCodes.Call, getTypeFromHandle);
                IL.Emit(OpCodes.Ceq);
                IL.Emit(OpCodes.Brfalse, skipAdd);

                foreach (var dependency in currentList)
                {
                    IL.Emit(OpCodes.Ldloc, listVariable);

                    var implementation = serviceMap[dependency];
                    implementation.Emit(dependency, serviceMap, getAllInstancesMethod);

                    IL.Emit(OpCodes.Stloc, currentService);

                    // Call IInitialize.Initialize(container) on the current service type
                    _initializer.Initialize(IL, module, currentService);

                    IL.Emit(OpCodes.Ldloc, currentService);
                    IL.Emit(OpCodes.Callvirt, addItem);
                }

                IL.Append(skipAdd);
            }

            var skipOtherContainerCall = IL.Create(OpCodes.Nop);

            var getNextContainer = module.ImportMethod<IMicroContainer>("get_NextContainer");
            var otherContainer = getAllInstancesMethod.AddLocal<IMicroContainer>();

            // var otherContainer = this.NextContainer;
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Callvirt, getNextContainer);
            IL.Emit(OpCodes.Stloc, otherContainer);

            // if (otherContainer != null && this != otherContainer) {
            IL.Emit(OpCodes.Ldloc, otherContainer);
            IL.Emit(OpCodes.Brfalse, skipOtherContainerCall);

            IL.Emit(OpCodes.Ldloc, otherContainer);
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Ceq);
            IL.Emit(OpCodes.Brtrue, skipOtherContainerCall);

            // var otherInstances = NextContainer.GetAllInstances(type);
            var otherGetAllInstancesMethod = module.ImportMethod<IMicroContainer>("GetAllInstances");

            IL.Emit(OpCodes.Ldloc, listVariable);
            IL.Emit(OpCodes.Ldloc, otherContainer);            
            IL.Emit(OpCodes.Ldarg_1);
            IL.Emit(OpCodes.Callvirt, otherGetAllInstancesMethod);

            // resultList.AddRange(otherInstances);
            var addRangeMethod = module.ImportMethod<List<object>>("AddRange");
            IL.Emit(OpCodes.Callvirt, addRangeMethod);

            // }

            // Cast the results down to an IEnumerable<object>
            var enumerableType = module.ImportType<IEnumerable<object>>();
            IL.Append(skipOtherContainerCall);
            IL.Emit(OpCodes.Ldloc, listVariable);
            IL.Emit(OpCodes.Isinst, enumerableType);                        
            IL.Emit(OpCodes.Ret);
        }
    }
}
