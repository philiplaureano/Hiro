using System;
using System.Collections.Generic;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents the basic implementation of a <see cref="ISingletonEmitter"/> instance.
    /// </summary>
    public class SingletonEmitter : ISingletonEmitter
    {
        /// <summary>
        /// The dictionary that maps dependencies to the corresponding singleton factory methods.
        /// </summary>
        private readonly Dictionary<IDependency, MethodDefinition> _entries = new Dictionary<IDependency, MethodDefinition>();

        /// <summary>
        /// Emits a service as a singleton type.
        /// </summary>
        /// <param name="targetMethod">The <see cref="IMicroContainer.GetInstance"/> method implementation.</param>
        /// <param name="dependency">The dependency that will be instantiated by the container.</param>
        /// <param name="implementation">The implementation that will be used to instantiate the dependency.</param>
        /// <param name="serviceMap">The service map the contains the current application dependencies.</param>
        public void EmitService(MethodDefinition targetMethod, IDependency dependency, 
                                IImplementation implementation, IDictionary<IDependency, IImplementation> serviceMap)
        {
            MethodDefinition getInstanceMethod = null;

            var worker = targetMethod.GetILGenerator();

            // Emit only one singleton per dependency and call
            // the singleton GetInstance() method on every subsequent emit call
            if (_entries.ContainsKey(dependency))
            {
                getInstanceMethod = _entries[dependency];
                worker.Emit(OpCodes.Call, getInstanceMethod);
                return;
            }

            var declaringType = targetMethod.DeclaringType;
            var module = declaringType.Module;

            var serviceType = dependency.ServiceType;

            var typeName = serviceType.Name;
            var singletonName = string.Format("{0}ServiceSingleton-{1}", typeName, dependency.GetHashCode());
            const TypeAttributes typeAttributes = TypeAttributes.NotPublic | TypeAttributes.AutoClass | TypeAttributes.Sealed |
                                                  TypeAttributes.BeforeFieldInit;
            var objectType = module.Import(typeof(object));

            var singletonType = AddDefaultSingletonConstructor(module, singletonName, typeAttributes, objectType);

            var instanceField = new FieldDefinition("__instance", FieldAttributes.Assembly | FieldAttributes.InitOnly | FieldAttributes.Static, objectType);

            DefineNestedType(module, singletonType, instanceField, serviceMap, implementation, dependency,
                             targetMethod);

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
            var constructor = singletonType.GetDefaultConstructor();

            constructor.IsFamilyOrAssembly = true;
            
            return singletonType;
        }

        /// <summary>
        /// Defines the nested type that will instantiate the actual singleton service instance.
        /// </summary>
        /// <param name="module">The module that will host the singleton type.</param>
        /// <param name="singletonType">The singleton type.</param>
        /// <param name="instanceField">The field that will hold the singleton instance.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="implementation">The implementation that will instantiate the dependency.</param>
        /// <param name="dependency">The dependency that will be instantiated by the singleton.</param>
        /// <param name="targetMethod">The method that will be used to instantiate the actual service instance.</param>
        private void DefineNestedType(ModuleDefinition module, TypeDefinition singletonType, FieldDefinition instanceField, IDictionary<IDependency, IImplementation> serviceMap, IImplementation implementation, IDependency dependency, MethodDefinition targetMethod)
        {
            var objectType = module.ImportType(typeof (object));
            var nestedName = string.Format("Nested-{0}", dependency.GetHashCode());

            const TypeAttributes nestedAttributes = TypeAttributes.NestedFamORAssem | TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.Class | TypeAttributes.AnsiClass;
            var nestedType = module.DefineClass(nestedName, "Hiro.Containers.Internal", nestedAttributes, objectType);
            singletonType.NestedTypes.Add(nestedType);

            nestedType.Fields.Add(instanceField);

            // Emit the static constructor body
            var cctor = DefineNestedConstructors(module, nestedType);
            
            EmitSingletonInstantiation(dependency, implementation, serviceMap, instanceField, cctor, module, targetMethod);                        
        }   

        /// <summary>
        /// Defines the instructions that will instantiate the singleton instance itself.
        /// </summary>
        /// <param name="dependency">The dependency that will be instantiated by the singleton.</param>
        /// <param name="implementation">The implementation that will instantiate the dependency.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        /// <param name="instanceField">The field that will hold the singleton instance.</param>
        /// <param name="cctor">The static constructor itself.</param>
        /// <param name="module">The target module.</param>
        /// <param name="targetMethod">The target method that will instantiate the service instance.</param>
        protected virtual void EmitSingletonInstantiation(IDependency dependency, 
            IImplementation implementation, 
            IDictionary<IDependency, IImplementation> serviceMap, 
            FieldDefinition instanceField, 
            MethodDefinition cctor, 
            ModuleDefinition module,
            MethodDefinition targetMethod)
        {
            var worker = cctor.GetILGenerator();
            implementation.Emit(dependency, serviceMap, cctor);

            worker.Emit(OpCodes.Stsfld, instanceField);
            worker.Emit(OpCodes.Ret);
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
            var defaultConstructor = nestedType.GetDefaultConstructor();
            defaultConstructor.IsPublic = true;            

            var cctor = DefineStaticConstructor(module, nestedType);
            cctor.IsPublic = true;

            return cctor;
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
            nestedType.Methods.Add(cctor);

            return cctor;
        }

        /// <summary>
        /// Defines the factory method on the singleton type.
        /// </summary>
        /// <param name="singletonType">The singleton type that will be generated by the emitter.</param>
        /// <param name="il">The <see cref="ILProcessor"/> instance that points to the target method body.</param>
        /// <param name="instanceField">The static field that holds the singleton instance.</param>
        /// <returns>The singleton type's GetInstance method.</returns>
        private MethodDefinition DefineGetInstance(TypeDefinition singletonType, ILProcessor il, FieldDefinition instanceField)
        {
            // Define the GetInstance method on the singleton type
            var getInstanceMethodAttributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;
            var getInstanceMethod = singletonType.DefineMethod("GetInstance", getInstanceMethodAttributes, typeof(object), new Type[0], new Type[0]);
            var singletonWorker = getInstanceMethod.GetILGenerator();

            singletonWorker.Emit(OpCodes.Ldsfld, instanceField);
            singletonWorker.Emit(OpCodes.Ret);

            return getInstanceMethod;
        }
    }
}