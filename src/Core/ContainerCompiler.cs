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
using FieldAttributes = Mono.Cecil.FieldAttributes;

namespace Hiro
{
    /// <summary>
    /// A class that compile a dependency graph into an inversion of control container.
    /// </summary>
    public class ContainerCompiler : IContainerCompiler
    {
        /// <summary>
        /// The class that will implement the GetInstance method.
        /// </summary>
        private readonly IGetInstanceMethodImplementor _getInstanceMethodImplementor;

        /// <summary>
        /// The class that will implement the Contains method.
        /// </summary>
        private readonly IContainsMethodImplementor _containsMethodImplementor;

        /// <summary>
        /// The class that will implement the GetAllInstances method.
        /// </summary>
        private readonly IGetAllInstancesMethodImplementor _getAllInstancesMethodImplementor;

        /// <summary>
        /// The class that will define the container type.
        /// </summary>
        private readonly ICreateContainerType _createContainerType;

        /// <summary>
        /// The class that will define the service map.
        /// </summary>
        private readonly IServiceMapBuilder _serviceMapBuilder;

        /// <summary>
        /// Initializes a new instance of the ContainerCompiler class.
        /// </summary>
        public ContainerCompiler()
            : this(new GetInstanceMethodImplementor(), new ContainsMethodImplementor(), new CreateContainerStub(), new ServiceMapBuilder(), new GetAllInstancesMethodImplementor())
        {
        }

        /// <summary>
        /// Initializes a new instance of the ContainerCompiler class.
        /// </summary>
        /// <param name="getInstanceMethodImplementor">The class that will implement the GetInstance method.</param>
        /// <param name="containsMethodImplementor">The class that will implement the Contains method.</param>
        /// <param name="createContainerType">The class that will define the container type.</param>
        /// <param name="serviceMapBuilder">The class that will define the service map.</param>
        /// <param name="getAllInstancesMethodImplementor">The class that will implement the GetAllInstances method.</param>
        public ContainerCompiler(IGetInstanceMethodImplementor getInstanceMethodImplementor,
            IContainsMethodImplementor containsMethodImplementor,
            ICreateContainerType createContainerType,
            IServiceMapBuilder serviceMapBuilder,
            IGetAllInstancesMethodImplementor getAllInstancesMethodImplementor)
        {
            _getInstanceMethodImplementor = getInstanceMethodImplementor;
            _containsMethodImplementor = containsMethodImplementor;
            _getAllInstancesMethodImplementor = getAllInstancesMethodImplementor;

            _createContainerType = createContainerType;
            _serviceMapBuilder = serviceMapBuilder;
        }

        /// <summary>
        /// Compiles a dependency graph into an IOC container.
        /// </summary>
        /// <param name="dependencyContainer">The <see cref="IDependencyContainer"/> instance that contains the services that will be instantiated by compiled container.</param>
        /// <param name="typeName">The name of the <see cref="IMicroContainer"/> type.</param>
        /// <param name="namespaceName">The namespace name that will be associated with the container type.</param>
        /// <param name="assemblyName">The name of the assembly that will contain the container type.</param>
        /// <returns>An assembly containing the compiled IOC container.</returns>
        public AssemblyDefinition Compile(string typeName, string namespaceName, string assemblyName, IDependencyContainer dependencyContainer)
        {
            var containerType = _createContainerType.CreateContainerType(typeName, namespaceName, assemblyName);
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

            var defaultConstructor = containerType.GetDefaultConstructor();
            var body = defaultConstructor.Body;
            var il = body.GetILProcessor();
            
            AddInitializationMap(containerType, module, fieldEmitter);

            InitializeContainerPlugins(module, dependencyContainer, il);

            il.Emit(OpCodes.Ret);

            _containsMethodImplementor.DefineContainsMethod(containerType, module, getServiceHash, jumpTargetField);
            _getInstanceMethodImplementor.DefineGetInstanceMethod(containerType, module, getServiceHash, jumpTargetField, serviceMap);
            _getAllInstancesMethodImplementor.DefineGetAllInstancesMethod(containerType, module, serviceMap);

            // Remove the NextContainer property stub
            var targetMethods = new List<MethodDefinition>();
            foreach (MethodDefinition method in containerType.Methods)
            {
                var methodName = method.Name;

                if (methodName == "get_NextContainer" || methodName == "set_NextContainer")
                    targetMethods.Add(method);
            }

            targetMethods.ForEach(m => containerType.Methods.Remove(m));

            // Add the NextContainer property
            containerType.AddProperty("NextContainer", typeof(IMicroContainer));

            return assembly;
        }

        /// <summary>
        /// Emits the instructions that introduce the <see cref="IContainerPlugin"/> instances to the current <see cref="IMicroContainer"/> instance.
        /// </summary>
        /// <param name="module">The target module.</param>
        /// <param name="dependencyContainer">The <see cref="IDependencyContainer"/> instance that contains the services that will be instantiated by compiled container.</param>
        /// <param name="il">The current <see cref="ILProcessor"/> instance that points to the current method body.</param>
        private void InitializeContainerPlugins(ModuleDefinition module, IDependencyContainer dependencyContainer, ILProcessor il)
        {
            var pluginDependencies = new List<IDependency>(dependencyContainer.Dependencies);

            Predicate<IDependency> predicate = dependency =>
                                                   {
                                                       if (dependency.ServiceType != typeof(IContainerPlugin))
                                                           return false;

                                                       var matches = dependencyContainer.GetImplementations(dependency, false);
                                                       var matchList = new List<IImplementation>(matches);

                                                       return matchList.Count > 0;
                                                   };

            pluginDependencies = pluginDependencies.FindAll(predicate);

            var getTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var getTypeFromHandle = module.Import(getTypeFromHandleMethod);
            var getInstanceMethod = module.ImportMethod<IMicroContainer>("GetInstance");
            foreach (var currentDependency in pluginDependencies)
            {
                var currentType = module.Import(currentDependency.ServiceType);
                var serviceName = currentDependency.ServiceName;

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldtoken, currentType);
                il.Emit(OpCodes.Call, getTypeFromHandle);

                var loadString = serviceName == null
                                     ? il.Create(OpCodes.Ldnull)
                                     : il.Create(OpCodes.Ldstr, serviceName);
                il.Append(loadString);

                il.Emit(OpCodes.Callvirt, getInstanceMethod);
                il.Emit(OpCodes.Pop);
            }
        }

        /// <summary>
        /// Modifies the default constructor to initialize the "__initializedServices" field so that it can ensure that all
        /// services called with the <see cref="IInitialize.Initialize"/> are initialized once per object lifetime.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <param name="module">The module.</param>
        /// <param name="fieldEmitter">The field builder.</param>
        private void AddInitializationMap(TypeDefinition containerType, ModuleDefinition module, FieldBuilder fieldEmitter)
        {
            var initializationMapType = module.Import(typeof(Dictionary<int, int>));
            var initializationMapField = new FieldDefinition("__initializedServices",
                                                             FieldAttributes.Private | FieldAttributes.InitOnly,
															 initializationMapType);
            containerType.Fields.Add(initializationMapField);

            var defaultConstructor = containerType.GetDefaultConstructor();
            var body = defaultConstructor.Body;

            // __initializedServices = new Dictionary<int, int>();
            var il = body.GetILProcessor();
            var dictionaryCtor = module.ImportConstructor<Dictionary<int, int>>();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, dictionaryCtor);
            il.Emit(OpCodes.Stfld, initializationMapField);
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
            var defaultContainerConstructor = targetType.GetDefaultConstructor();

            var body = defaultContainerConstructor.Body;
            var il = body.GetILProcessor();

            // Remove the last instruction and replace it with the jump entry 
            // initialization instructions
            RemoveLastInstruction(body);

            // Initialize the jump targets in the default container constructor
            var getTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var getTypeFromHandle = module.Import(getTypeFromHandleMethod);

            // __jumpTargets = new Dictionary<int, int>();
            var dictionaryCtor = module.ImportConstructor<Dictionary<int, int>>();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Newobj, dictionaryCtor);
            il.Emit(OpCodes.Stfld, jumpTargetField);

            var addMethod = module.ImportMethod<Dictionary<int, int>>("Add");
            var index = 0;
            foreach (var dependency in serviceMap.Keys)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, jumpTargetField);

                var serviceType = dependency.ServiceType;
                var serviceTypeRef = module.Import(serviceType);

                // Push the service type
                il.Emit(OpCodes.Ldtoken, serviceTypeRef);
                il.Emit(OpCodes.Call, getTypeFromHandle);

                // Push the service name
                var pushName = dependency.ServiceName == null ? il.Create(OpCodes.Ldnull) : il.Create(OpCodes.Ldstr, dependency.ServiceName);
                il.Append(pushName);

                // Calculate the hash code using the service type and service name
                il.Emit(OpCodes.Call, getServiceHash);

                // Map the current dependency to the index
                // that will be used in the GetInstance switch statement
                jumpTargets[dependency] = index;

                il.Emit(OpCodes.Ldc_I4, index++);
                il.Emit(OpCodes.Callvirt, addMethod);
            }
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
                instructions.RemoveAt(instructions.Count - 1);
            }
        }
    }
}
