using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents a class that creates singleton services.
    /// </summary>
    internal class ContainerBasedSingletonEmitter : SingletonEmitter
    {
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
        protected override void EmitSingletonInstantiation(IDependency dependency,
            IImplementation implementation, 
            IDictionary<IDependency, IImplementation> serviceMap, 
            FieldDefinition instanceField, 
            MethodDefinition cctor, 
            ModuleDefinition module,
            MethodDefinition targetMethod)
        {
            var containerType = targetMethod.DeclaringType;
            var containerLocal = AddContainerVariable(module, cctor);
            var containerConstructor = containerType.GetDefaultConstructor();
            var il = cctor.GetILGenerator();
            SaveContainerInstance(il, containerConstructor, containerLocal);

            base.EmitSingletonInstantiation(dependency, implementation, serviceMap, instanceField, cctor, module, targetMethod);

            ReplaceContainerCalls(cctor, containerLocal, il);
        }

        private static void SaveContainerInstance(ILProcessor il, MethodDefinition containerConstructor,
                                                 VariableDefinition containerLocal)
        {
            il.Emit(OpCodes.Newobj, containerConstructor);
            il.Emit(OpCodes.Stloc, containerLocal);
        }

        private VariableDefinition AddContainerVariable(ModuleDefinition module, MethodDefinition cctor)
        {
            var microContainerInterfaceType = module.ImportType<IMicroContainer>();
            var containerLocal = new VariableDefinition(microContainerInterfaceType);

            cctor.Body.Variables.Add(containerLocal);
            return containerLocal;
        }

        /// <summary>
        /// Converts the self calls made to the <see cref="IMicroContainer.GetInstance"/> instance into method calls that use
        /// a <see cref="IMicroContainer"/> instance stored in a local variable.
        /// </summary>
        /// <param name="cctor">The static constructor.</param>
        /// <param name="containerLocal">The variable that will store the <see cref="IMicroContainer"/> instance.</param>
        /// <param name="il">The <see cref="ILProcessor" /> that points to the target method body.</param>
        private static void ReplaceContainerCalls(MethodDefinition cctor, VariableDefinition containerLocal, ILProcessor il)
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
                il.Replace(taggedInstructions.Dequeue(), il.Create(OpCodes.Ldloc, containerLocal));
            }
        }
    }
}
