using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Compilers;
using Hiro.Containers;
using Hiro.Interfaces;
using LinFu.Reflection.Emit;
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
        /// Compiles a dependency graph into an IOC container.
        /// </summary>
        /// <param name="dependencyContainer">The <see cref="IDependencyContainer"/> instance that contains the services that will be instantiated by compiled container.</param>
        /// <returns>An assembly containing the compiled IOC container.</returns>
        public AssemblyDefinition Compile(IDependencyContainer dependencyContainer)
        {
            TypeDefinition containerType = CreateContainerStub();

            var module = containerType.Module;
            var assembly = module.Assembly;

            var hashEmitter = new ServiceHashEmitter();
            var getServiceHash = hashEmitter.AddGetServiceHashMethodTo(containerType, false);

            var fieldType = module.Import(typeof(Dictionary<int, int>));
            var fieldEmitter = new FieldBuilder();
            var jumpTargetField = fieldEmitter.AddField(containerType, "__jumpTargets", fieldType);
            var serviceMap = GetAvailableServices(dependencyContainer);
            var jumpTargets = new Dictionary<IDependency, int>();

            // Map the switch labels in the default constructor
            AddJumpEntries(module, jumpTargetField, containerType, getServiceHash, serviceMap, jumpTargets);

            DefineContainsMethod(containerType, module, getServiceHash, jumpTargetField);

            // Implement the GetInstance method
            var getInstanceMethod = (from MethodDefinition m in containerType.Methods
                                    where m.Name == "GetInstance"
                                    select m).First();

            var body = getInstanceMethod.Body;
            var worker = body.CilWorker;

            body.Instructions.Clear();

            ReturnNullIfServiceDoesNotExist(module, worker);
                        
            var hashVariable = getInstanceMethod.AddLocal<int>();

            // Calculate the service hash code
            EmitCalculateServiceHash(getServiceHash, worker);
            worker.Emit(OpCodes.Stloc, hashVariable);

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldfld, jumpTargetField);
            worker.Emit(OpCodes.Ldloc, hashVariable);

            // Calculate the target label index
            var getItem = module.ImportMethod<Dictionary<int, int>>("get_Item");
            worker.Emit(OpCodes.Callvirt, getItem);

            // Define the jump labels
            var jumpLabels = new List<Instruction>();
            var entryCount = serviceMap.Count;
            for (int i = 0; i < entryCount; i++)
            {
                var newLabel = worker.Create(OpCodes.Nop);
                jumpLabels.Add(newLabel);
            }

            var endLabel = worker.Emit(OpCodes.Nop);

            worker.Emit(OpCodes.Switch, jumpLabels.ToArray());

            var index = 0;
            foreach (var dependency in serviceMap.Keys)
            {
                // Mark the jump label
                var label = jumpLabels[index];
                worker.Append(label);

                // Emit the implementation
                var implementation = serviceMap[dependency];
                implementation.Emit(getInstanceMethod, dependency, serviceMap);

                worker.Emit(OpCodes.Br, endLabel);
                index++;
            }

            worker.Append(endLabel);
            worker.Emit(OpCodes.Ret);

            return assembly;
        }

        /// <summary>
        /// Emits the instructions that ensure that the target method returns null if the container cannot create the current service name and service type.
        /// </summary>
        /// <param name="module">The target module.</param>
        /// <param name="worker">The worker that points to the method body of the GetInstance method.</param>
        private static void ReturnNullIfServiceDoesNotExist(ModuleDefinition module, CilWorker worker)
        {
            var skipReturnNull = worker.Emit(OpCodes.Nop);
            var containsMethod = module.ImportMethod<IMicroContainer>("Contains");

            // if (!Contains(serviceType, serviceName))
            // return null;
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Ldarg_2);
            worker.Emit(OpCodes.Callvirt, containsMethod);
            worker.Emit(OpCodes.Brtrue, skipReturnNull);

            worker.Emit(OpCodes.Ldnull);
            worker.Emit(OpCodes.Ret);
            worker.Append(skipReturnNull);
        }

        /// <summary>
        /// Emits the body of the <see cref="IMicroContainer.Contains"/> method implementation.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <param name="module">The target module.</param>
        /// <param name="getServiceHash">The method that will be used to determine the hash code of the current service.</param>
        /// <param name="jumpTargetField">The field that contains the list of jump entries.</param>
        private static void DefineContainsMethod(TypeDefinition containerType, ModuleDefinition module, MethodDefinition getServiceHash, FieldDefinition jumpTargetField)
        {
            // Override the Contains method stub
            var containsMethod = (from MethodDefinition m in containerType.Methods
                                  where m.Name == "Contains"
                                  select m).First();

            var body = containsMethod.Body;
            var worker = body.CilWorker;

            // Remove the stub implementation
            body.Instructions.Clear();

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldfld, jumpTargetField);

            EmitCalculateServiceHash(getServiceHash, worker);

            var containsEntry = module.ImportMethod<Dictionary<int, int>>("ContainsKey");
            worker.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Emits the instructions that calculate the hash code of a given service type and service name.
        /// </summary>
        /// <param name="getServiceHash">The method that will be used to calculate the hash code.</param>
        /// <param name="worker">The worker that points to the target method body.</param>
        private static void EmitCalculateServiceHash(MethodDefinition getServiceHash, CilWorker worker)
        {
            // Push the service type
            worker.Emit(OpCodes.Ldarg_1);

            // Push the service name
            worker.Emit(OpCodes.Ldarg_2);

            // Calculate the hash code using the service type and service name
            worker.Emit(OpCodes.Call, getServiceHash);
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
        private static void AddJumpEntries(ModuleDefinition module, FieldDefinition jumpTargetField, TypeDefinition targetType, MethodReference getServiceHash, Dictionary<IDependency, IImplementation> serviceMap, Dictionary<IDependency, int> jumpTargets)
        {
            var defaultContainerConstructor = targetType.Constructors[0];

            var body = defaultContainerConstructor.Body;
            var worker = body.CilWorker;

            // Remove the last instruction and replace it with the jump entry 
            // initialization instructions
            RemoveLastInstruction(body);

            // Initialize the jump targets in the default container constructor
            var getTypeFromHandle = module.ImportMethod<Type>("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);

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

        /// <summary>
        /// Creates a stub <see cref="IMicroContainer"/> implementation.
        /// </summary>
        /// <returns>A <see cref="TypeDefinition"/> with a stubbed <see cref="IMicroContainer"/> implementation.</returns>
        private static TypeDefinition CreateContainerStub()
        {
            var assemblyBuilder = new AssemblyBuilder();
            var assembly = assemblyBuilder.CreateAssembly("Hiro.CompiledContainers", AssemblyKind.Dll);
            var module = assembly.MainModule;

            var objectType = module.Import(typeof(object));
            var containerInterfaceType = module.Import(typeof(IMicroContainer));
            var typeBuilder = new ContainerTypeBuilder();
            var containerType = typeBuilder.CreateType("MicroContainer", "Hiro.Containers", objectType, assembly, containerInterfaceType);

            // Add a stub implementation for the IMicroContainer interface
            var stubBuilder = new InterfaceStubBuilder();
            stubBuilder.AddStubImplementationFor(typeof(IMicroContainer), containerType);

            return containerType;
        }

        /// <summary>
        /// Obtains the list of available services from the given <paramref name="dependencyContainer"/>.
        /// </summary>
        /// <param name="dependencyContainer">The container that contains the list of services.</param>
        /// <returns>A dictionary that maps dependencies to their respective implementations.</returns>
        private static Dictionary<IDependency, IImplementation> GetAvailableServices(IDependencyContainer dependencyContainer)
        {
            Dictionary<IDependency, IImplementation> serviceMap = new Dictionary<IDependency, IImplementation>();

            var dependencies = dependencyContainer.Dependencies;
            foreach (var dependency in dependencies)
            {
                var implementations = dependencyContainer.GetImplementations(dependency, false);
                var implementation = implementations.FirstOrDefault();
                if (implementation == null)
                    continue;

                serviceMap[dependency] = implementation;
            }

            return serviceMap;
        }
    }
}
