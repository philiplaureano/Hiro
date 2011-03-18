using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IGetInstanceMethodImplementor"/> interface.
    /// </summary>
    internal class GetInstanceMethodImplementor : IGetInstanceMethodImplementor
    {
        private readonly IServiceInitializer _initializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInstanceMethodImplementor"/> class.
        /// </summary>
        public GetInstanceMethodImplementor()
            : this(new ServiceInitializer())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetInstanceMethodImplementor"/> class.
        /// </summary>
        /// <param name="initializer">The <see cref="IServiceInitializer"/> instance that will initialize the target given services.</param>
        public GetInstanceMethodImplementor(IServiceInitializer initializer)
        {
            _initializer = initializer;
        }

        /// <summary>
        /// Defines the <see cref="IMicroContainer.GetInstance"/> method implementation for the container type.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <param name="module">The target module.</param>
        /// <param name="getServiceHash">The GetServiceHash method.</param>
        /// <param name="jumpTargetField">The field that will store the jump target indexes.</param>
        /// <param name="serviceMap">The service map that contains the list of existing services.</param>
        public void DefineGetInstanceMethod(TypeDefinition containerType, ModuleDefinition module, MethodDefinition getServiceHash, FieldDefinition jumpTargetField, IDictionary<IDependency, IImplementation> serviceMap)
        {
            var targetMethods = new List<MethodDefinition>();
            foreach (MethodDefinition method in containerType.Methods)
            {
                if (method.Name != "GetInstance")
                    continue;

                targetMethods.Add(method);
            }

            MethodDefinition getInstanceMethod = targetMethods[0];
            var body = getInstanceMethod.Body;
            body.InitLocals = true;

            var il = body.GetILProcessor();

            body.Instructions.Clear();

            DefineServices(serviceMap, getInstanceMethod, jumpTargetField, getServiceHash, il);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Emits the instructions that will instantiate the target service.
        /// </summary>
        /// <param name="getInstanceMethod">The method that will instantiate the target type.</param>
        /// <param name="dependency">The target dependency</param>       
        /// <param name="implementation">The implementation that will instantiate the dependency.</param>
        /// <param name="serviceMap">The service map that contains the list of dependencies in the application.</param>
        protected virtual void EmitService(MethodDefinition getInstanceMethod, IDependency dependency, IImplementation implementation, IDictionary<IDependency, IImplementation> serviceMap)
        {
            implementation.Emit(dependency, serviceMap, getInstanceMethod);
        }

        /// <summary>
        /// Defines the instructions that create each service type in the <paramref name="serviceMap"/>.
        /// </summary>
        /// <param name="serviceMap">The service map that contains the list of application dependencies.</param>
        /// <param name="getInstanceMethod">The method that will be used to instantiate the service types.</param>
        /// <param name="jumpTargetField">The field that holds the dictionary with the switch table target instructions.</param>
        /// <param name="getServiceHash">The method that calculates the service hash code.</param>
        /// <param name="il">The <see cref="ILProcessor"/> that points to the body of the factory method.</param>
        private void DefineServices(IDictionary<IDependency, IImplementation> serviceMap, MethodDefinition getInstanceMethod, FieldDefinition jumpTargetField, MethodDefinition getServiceHash, ILProcessor il)
        {
            var endLabel = Instruction.Create(OpCodes.Nop);
            il.Append(endLabel);

            var body = il.Body;
            body.InitLocals = true;

            var method = body.Method;
            var returnValue = method.AddLocal<object>();
            var declaringType = method.DeclaringType;
            var module = declaringType.Module;

            var getTypeFromHandle = module.ImportMethod<Type>("GetTypeFromHandle", BindingFlags.Public | BindingFlags.Static);
            var equalsMethod = typeof(string).GetMethod("CompareOrdinal", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(string) }, null);
            var stringEquals = module.Import(equalsMethod);

            var jumpTargets = new Dictionary<IDependency, Instruction>();
            foreach (var dependency in serviceMap.Keys)
            {
                jumpTargets[dependency] = il.Create(OpCodes.Nop);
            }
            var getItemMethod = module.ImportMethod<Dictionary<int, int>>("get_Item");
            var serviceHash = method.AddLocal<int>();
            var skipCreate = il.Create(OpCodes.Nop);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, jumpTargetField);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Call, getServiceHash);
            il.Emit(OpCodes.Stloc, serviceHash);

            var contains = module.ImportMethod<Dictionary<int, int>>("ContainsKey");
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, jumpTargetField);
            il.Emit(OpCodes.Ldloc, serviceHash);
            il.Emit(OpCodes.Callvirt, contains);
            il.Emit(OpCodes.Brfalse, skipCreate);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, jumpTargetField);
            il.Emit(OpCodes.Ldloc, serviceHash);
            il.Emit(OpCodes.Callvirt, getItemMethod);

            var switchLabels = new List<Instruction>(jumpTargets.Values);
            var labels = switchLabels.ToArray();
            il.Emit(OpCodes.Switch, labels);
            il.Emit(OpCodes.Br, skipCreate);

            foreach (var dependency in serviceMap.Keys)
            {
                // Set the jump target
                var currentLabel = jumpTargets[dependency];
                il.Append(currentLabel);

                var serviceType = module.ImportType(dependency.ServiceType);

                // Emit the implementation
                var implementation = serviceMap[dependency];
                EmitService(getInstanceMethod, dependency, implementation, serviceMap);

                if (serviceType.IsValueType)
                    il.Emit(OpCodes.Box, serviceType);

                il.Emit(OpCodes.Stloc, returnValue);

                var serviceInstance = returnValue;

                // Call IInitialize.Initialize(this) on the current type
                if (_initializer != null)
                    _initializer.Initialize(il, module, serviceInstance);

                il.Emit(OpCodes.Ldloc, returnValue);

                il.Emit(OpCodes.Br, endLabel);
            }

            il.Append(skipCreate);

            var getNextContainer = module.ImportMethod<IMicroContainer>("get_NextContainer");
            var otherContainer = method.AddLocal<IMicroContainer>();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, getNextContainer);
            il.Emit(OpCodes.Stloc, otherContainer);

            // if (otherContainer != null ) {
            var skipOtherContainerCall = il.Create(OpCodes.Nop);
            il.Emit(OpCodes.Ldloc, otherContainer);
            il.Emit(OpCodes.Brfalse, skipOtherContainerCall);

            // Prevent the container from calling itself
            il.Emit(OpCodes.Ldloc, otherContainer);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Brtrue, skipOtherContainerCall);

            var otherGetInstanceMethod = module.ImportMethod<IMicroContainer>("GetInstance");

            // return otherContainer.GetInstance(type, key);
            il.Emit(OpCodes.Ldloc, otherContainer);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Callvirt, otherGetInstanceMethod);
            il.Emit(OpCodes.Br, endLabel);
            // }

            il.Append(skipOtherContainerCall);
            il.Emit(OpCodes.Ldnull);
            il.Append(endLabel);
        }
    }
}
