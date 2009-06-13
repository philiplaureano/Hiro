using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Compilers;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NGenerics.DataStructures.General;

namespace Hiro
{
    /// <summary>
    /// A class that compile a dependency graph into an inversion of control container.
    /// </summary>
    public class ContainerCompiler
    {
        /// <summary>
        /// The class that will implement the GetInstance method.
        /// </summary>
        private IGetInstanceMethodImplementor _getInstanceMethodImplementor;

        /// <summary>
        /// The class that will implement the Contains method.
        /// </summary>
        private IContainsMethodImplementor _containsMethodImplementor;

        /// <summary>
        /// The class that will define the container type.
        /// </summary>
        private ICreateContainerType _createContainerType;

        /// <summary>
        /// The class that will define the service map.
        /// </summary>
        private IServiceMapBuilder _serviceMapBuilder;

        /// <summary>
        /// Initializes a new instance of the ContainerCompiler class.
        /// </summary>
        public ContainerCompiler()
            : this(new GetInstanceMethodImplementor(), new ContainsMethodImplementor(), new CreateContainerStub(), new ServiceMapBuilder())
        {
        }

        /// <summary>
        /// Initializes a new instance of the ContainerCompiler class.
        /// </summary>
        /// <param name="getInstanceMethodImplementor">The class that will implement the GetInstance method.</param>
        /// <param name="containsMethodImplementor">The class that will implement the Contains method.</param>
        /// <param name="createContainerType">The class that will define the container type.</param>
        /// <param name="serviceMapBuilder">The class that will define the service map.</param>
        public ContainerCompiler(IGetInstanceMethodImplementor getInstanceMethodImplementor, IContainsMethodImplementor containsMethodImplementor, ICreateContainerType createContainerType, IServiceMapBuilder serviceMapBuilder)
        {
            _getInstanceMethodImplementor = getInstanceMethodImplementor;
            _containsMethodImplementor = containsMethodImplementor;
            _createContainerType = createContainerType;
            _serviceMapBuilder = serviceMapBuilder;
        }

        /// <summary>
        /// Compiles a dependency graph into an IOC container.
        /// </summary>
        /// <param name="dependencyContainer">The <see cref="IDependencyContainer"/> instance that contains the services that will be instantiated by compiled container.</param>
        /// <returns>An assembly containing the compiled IOC container.</returns>
        public AssemblyDefinition Compile(IDependencyContainer dependencyContainer)
        {
            TypeDefinition containerType = _createContainerType.CreateContainerType("MicroContainer", "Hiro.Containers", "Hiro.CompiledContainers");

            var module = containerType.Module;
            var assembly = module.Assembly;

            var hashEmitter = new ServiceHashEmitter();
            var getServiceHash = hashEmitter.AddGetServiceHashMethodTo(containerType, false);

            var fieldType = module.Import(typeof(Dictionary<int, int>));
            var fieldEmitter = new FieldBuilder();
            var jumpTargetField = fieldEmitter.AddField(containerType, "__jumpTargets", fieldType);
            var serviceMap = _serviceMapBuilder.GetAvailableServices(dependencyContainer);
            var jumpTargets = new Dictionary<IDependency, int>();

            // Map the switch labels in the default constructor
            AddJumpEntries(module, jumpTargetField, containerType, getServiceHash, serviceMap, jumpTargets);

            _containsMethodImplementor.DefineContainsMethod(containerType, module, getServiceHash, jumpTargetField);

            _getInstanceMethodImplementor.DefineGetInstanceMethod(containerType, module, getServiceHash, jumpTargetField, serviceMap);

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
                    worker.Emit(OpCodes.Callvirt, addItem);
                }                

                worker.Append(skipAdd);
            }

            // Cast the results down to an IEnumerable<object>
            var enumerableType = module.ImportType<IEnumerable<object>>();
            worker.Emit(OpCodes.Ldloc, listVariable);
            worker.Emit(OpCodes.Isinst, enumerableType);
            worker.Emit(OpCodes.Ret);

            return assembly;
        }

        /// <summary>
        /// Modifies the default constructor of a container type so that the jump labels used in the <see cref="IMicroContainer.GetInstance"/> implementation
        /// will be precalculated every time the compiled container is instantiated.
        /// </summary>
        /// <param name="module">The target module.</param>
        /// <param name="jumpTargetField">The field that holds the jump entries.</param>
        /// <param name="targetType">The container type.</param>
        /// <param name="getServiceHash">The hash calculation method.</param>
        /// <param name="serviceMap">The collection that contains the current list of dependencies and their respective implementations.</param>
        /// <param name="jumpTargets">A dictionary that maps dependencies to their respective label indexes.</param>
        private static void AddJumpEntries(ModuleDefinition module, FieldDefinition jumpTargetField, TypeDefinition targetType, MethodReference getServiceHash, IDictionary<IDependency, IImplementation> serviceMap, IDictionary<IDependency, int> jumpTargets)
        {
            var defaultContainerConstructor = targetType.Constructors[0];

            var body = defaultContainerConstructor.Body;
            var worker = body.CilWorker;

            // Remove the last instruction and replace it with the jump entry 
            // initialization instructions
            RemoveLastInstruction(body);

            // Initialize the jump targets in the default container constructor
            var getTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var getTypeFromHandle = module.Import(getTypeFromHandleMethod);

            // __jumpTargets = new Dictionary<int, int>();
            var dictionaryCtor = module.ImportConstructor<Dictionary<int, int>>();
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Newobj, dictionaryCtor);
            worker.Emit(OpCodes.Stfld, jumpTargetField);

            var addMethod = module.ImportMethod<Dictionary<int, int>>("Add");
            var index = 0;
            foreach (var dependency in serviceMap.Keys)
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, jumpTargetField);

                var serviceType = dependency.ServiceType;
                var serviceTypeRef = module.Import(serviceType);

                // Push the service type
                worker.Emit(OpCodes.Ldtoken, serviceTypeRef);
                worker.Emit(OpCodes.Call, getTypeFromHandle);

                // Push the service name
                var pushName = dependency.ServiceName == null ? worker.Create(OpCodes.Ldnull) : worker.Create(OpCodes.Ldstr, dependency.ServiceName);
                worker.Append(pushName);

                // Calculate the hash code using the service type and service name
                worker.Emit(OpCodes.Call, getServiceHash);

                // Map the current dependency to the index
                // that will be used in the GetInstance switch statement
                jumpTargets[dependency] = index;

                worker.Emit(OpCodes.Ldc_I4, index++);
                worker.Emit(OpCodes.Callvirt, addMethod);
            }

            worker.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Removes the last instruction from the given method body.
        /// </summary>
        /// <param name="body">The target method body.</param>
        private static void RemoveLastInstruction(Mono.Cecil.Cil.MethodBody body)
        {
            var instructions = body.Instructions;

            if (instructions.Count > 0)
            {
                var lastInstruction = instructions[0];
                instructions.RemoveAt(instructions.Count - 1);
            }
        }
    }
}
