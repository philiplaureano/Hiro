using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using LinFu.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents a class that creates singleton services.
    /// </summary>
    internal class SingletonEmitter
    {
        /// <summary>
        /// The dictionary that maps dependencies to the corresponding singleton factory methods.
        /// </summary>
        private Dictionary<IDependency, MethodDefinition> _entries = new Dictionary<IDependency, MethodDefinition>();

        /// <summary>
        /// Emits a service as a singleton type.
        /// </summary>
        /// <param name="newMethod">The <see cref="IMicroContainer.GetInstance"/> method implementation.</param>
        /// <param name="dependency">The dependency that will be instantiated by the container.</param>
        /// <param name="implementation">The implementation that will be used to instantiate the dependency.</param>
        /// <param name="serviceMap">The service map the contains the current application dependencies.</param>
        public void EmitService(MethodDefinition newMethod, IDependency dependency, IImplementation implementation, IDictionary<IDependency, IImplementation> serviceMap)
        {
            MethodDefinition getInstanceMethod = null;

            var worker = newMethod.GetILGenerator();

            // Emit only one singleton per dependency and call
            // the singleton GetInstance() method on every subsequent emit call
            if (_entries.ContainsKey(dependency))
            {
                getInstanceMethod = _entries[dependency];
                worker.Emit(OpCodes.Call, getInstanceMethod);
                return;
            }

            var declaringType = newMethod.DeclaringType;
            var module = declaringType.Module;
            var containerType = newMethod.DeclaringType;
            var serviceType = dependency.ServiceType;

            var typeName = serviceType.Name;
            var singletonName = string.Format("{0}ServiceSingleton-{1}", typeName, dependency.GetHashCode());
            var typeAttributes = TypeAttributes.NotPublic | TypeAttributes.AutoClass | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;
            var objectType = module.ImportType<object>();

            var singletonType = AddDefaultSingletonConstructor(module, singletonName, typeAttributes, objectType);
            var instanceField = new FieldDefinition("__instance", objectType, FieldAttributes.Assembly | FieldAttributes.InitOnly | FieldAttributes.Static);

            DefineNestedType(module, containerType, dependency, implementation, serviceMap, objectType, singletonType, instanceField);

            getInstanceMethod = DefineGetInstance(singletonType, worker, instanceField);

            worker.Emit(OpCodes.Call, getInstanceMethod);

            var serviceTypeRef = module.Import(serviceType);
            worker.Emit(OpCodes.Unbox_Any, serviceTypeRef);

            // Cache the singleton method
            _entries[dependency] = getInstanceMethod;
        }        

        /// <summary>
        /// Adds a default constructor to the singleton type.
        /// </summary>
        /// <param name="module">The module that will host the singleton type.</param>
        /// <param name="singletonName">The name of the singleton.</param>
        /// <param name="typeAttributes">The type attributes that describes the singleton type.</param>
        /// <param name="objectType">The object ty pe.</param>
        /// <returns>A <see cref="TypeDefinition"/> that represents the singleton type.</returns>
        private static TypeDefinition AddDefaultSingletonConstructor(ModuleDefinition module, string singletonName, TypeAttributes typeAttributes, TypeReference objectType)
        {
            // Add a default constructor and make it private
            var singletonType = module.DefineClass(singletonName, "Hiro.Containers.Internal", typeAttributes, objectType);
            singletonType.AddDefaultConstructor();
            singletonType.Constructors[0].IsPublic = false;

            return singletonType;
        }

        /// <summary>
        /// Defines the nested type that will instantiate the actual singleton service instance.
        /// </summary>
        /// <param name="module">The module that will host the singleton type.</param>
        /// <param name="containerType">The container type.</param>
        /// <param name="dependency">The dependency that will be instantiated by the singleton.</param>
        /// <param name="implementation">The implementation that will instantiate the dependency.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="objectType">The object type.</param>
        /// <param name="singletonType">The singleton type.</param>
        /// <param name="instanceField">The field that will hold the singleton instance.</param>
        private static void DefineNestedType(ModuleDefinition module, TypeDefinition containerType, IDependency dependency, IImplementation implementation, IDictionary<IDependency, IImplementation> serviceMap, TypeReference objectType, TypeDefinition singletonType, FieldDefinition instanceField)
        {
            var nestedName = string.Format("Nested-{0}", dependency.GetHashCode());
            var nestedAttributes = TypeAttributes.NestedPrivate | TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.Class | TypeAttributes.AnsiClass;
            var nestedType = module.DefineClass(nestedName, "Hiro.Containers.Internal", nestedAttributes, objectType);
            singletonType.NestedTypes.Add(nestedType);

            nestedType.Fields.Add(instanceField);
            var microContainerInterfaceType = module.ImportType<IMicroContainer>();

            // Emit the static constructor body
            var cctor = DefineNestedConstructors(module, nestedType);
            var worker = cctor.GetILGenerator();
            var containerLocal = new VariableDefinition(microContainerInterfaceType);
            cctor.Body.Variables.Add(containerLocal);
            var containerConstructor = containerType.Constructors[0];

            DefineNestedStaticConstructorBody(module, dependency, implementation, serviceMap, instanceField, cctor, containerLocal, containerConstructor);
        }

        /// <summary>
        /// Defines the instructions that will instantiate the singleton instance itself.
        /// </summary>
        /// <param name="module">The module that will host the singleton type.</param>
        /// <param name="dependency">The dependency that will be instantiated by the singleton.</param>
        /// <param name="implementation">The implementation that will instantiate the dependency.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="instanceField">The field that will hold the singleton instance.</param>
        /// <param name="cctor">The static constructor itself.</param>
        /// <param name="containerLocal">The local variable that will hold the container instance.</param>
        /// <param name="containerConstructor">The constructor that will be used to instantiate the container.</param>
        private static void DefineNestedStaticConstructorBody(
            ModuleDefinition module,
            IDependency dependency,
            IImplementation implementation,
            IDictionary<IDependency, IImplementation> serviceMap,
            FieldDefinition instanceField,
            MethodDefinition cctor,
            VariableDefinition containerLocal,
            MethodDefinition containerConstructor)
        {
            var worker = cctor.GetILGenerator();
            worker.Emit(OpCodes.Newobj, containerConstructor);
            worker.Emit(OpCodes.Stloc, containerLocal);

            implementation.Emit(dependency, serviceMap, cctor);

            worker.Emit(OpCodes.Stsfld, instanceField);
            worker.Emit(OpCodes.Ret);

            ReplaceContainerCalls(cctor, containerLocal, worker);
        }

        /// <summary>
        /// Defines the nested constructors for the singleton type.
        /// </summary>
        /// <param name="module">The target module.</param>
        /// <param name="nestedType">The nested type.</param>
        /// <returns>The static singleton constructor.</returns>
        private static MethodDefinition DefineNestedConstructors(ModuleDefinition module, TypeDefinition nestedType)
        {
            // Define the constructor for the nested t ype
            nestedType.AddDefaultConstructor();
            var defaultConstructor = nestedType.Constructors[0];
            defaultConstructor.IsPublic = false;
            defaultConstructor.IsPrivate = true;

            var cctor = DefineStaticConstructor(module, nestedType);
            cctor.Body.InitLocals = true;

            return cctor;
        }

        /// <summary>
        /// Converts the self calls made to the <see cref="IMicroContainer.GetInstance"/> instance into method calls that use
        /// a <see cref="IMicroContainer"/> instance stored in a local variable.
        /// </summary>
        /// <param name="cctor">The static constructor.</param>
        /// <param name="containerLocal">The variable that will store the <see cref="IMicroContainer"/> instance.</param>
        /// <param name="worker">The worker that points to the target method body.</param>
        private static void ReplaceContainerCalls(MethodDefinition cctor, VariableDefinition containerLocal, CilWorker worker)
        {
            // Replace the calls to the 'this' pointer (ldarg0) with
            // the local MicroContainer instance
            var taggedInstructions = new Queue<Instruction>();
            foreach (Instruction currentInstruction in cctor.Body.Instructions)
            {
                if (currentInstruction.OpCode != OpCodes.Ldarg_0)
                    continue;

                taggedInstructions.Enqueue(currentInstruction);
            }

            while (taggedInstructions.Count > 0)
            {
                worker.Replace(taggedInstructions.Dequeue(), worker.Create(OpCodes.Ldloc, containerLocal));
            }
        }

        /// <summary>
        /// Defines the static constructor for the nested type.
        /// </summary>
        /// <param name="module">The target module.</param>
        /// <param name="nestedType">The nested type itself.</param>
        /// <returns>The nested static constructor itself.</returns>
        private static MethodDefinition DefineStaticConstructor(ModuleDefinition module, TypeDefinition nestedType)
        {
            // Define the nested static constructor
            var voidType = module.ImportType(typeof(void));
            var attributes = MethodAttributes.Private |
                           MethodAttributes.HideBySig |
                           MethodAttributes.Static |
                           MethodAttributes.RTSpecialName |
                           MethodAttributes.SpecialName;

            var cctor = new MethodDefinition(".cctor", attributes, voidType);

            cctor.ImplAttributes = Mono.Cecil.MethodImplAttributes.IL | Mono.Cecil.MethodImplAttributes.Managed;
            nestedType.Constructors.Add(cctor);

            return cctor;
        }

        /// <summary>
        /// Defines the factory method on the singleton type.
        /// </summary>
        /// <param name="singletonType">The singleton type that will be generated by the emitter.</param>
        /// <param name="worker">The <see cref="CilWorker"/> instance that points to the target method body.</param>
        /// <param name="instanceField">The static field that holds the singleton instance.</param>
        /// <returns>The singleton type's GetInstance method.</returns>
        private MethodDefinition DefineGetInstance(TypeDefinition singletonType, CilWorker worker, FieldDefinition instanceField)
        {
            // Define the GetInstance method on the singleton type
            var getInstanceMethodAttributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;
            var getInstanceMethod = singletonType.DefineMethod("GetInstance", getInstanceMethodAttributes, typeof(object));
            var singletonWorker = getInstanceMethod.GetILGenerator();

            singletonWorker.Emit(OpCodes.Ldsfld, instanceField);
            singletonWorker.Emit(OpCodes.Ret);

            return getInstanceMethod;
        }
    }
}
