using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Compilers;
using Hiro.Containers;
using LinFu.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using Hiro.UnitTests.SampleDomain;
using Hiro.Interfaces;
using Moq;
using Hiro.Implementations;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class CompilerTests : BaseFixture
    {
        private AssemblyBuilder _assemblyBuilder;

        protected override void OnInit()
        {
            _assemblyBuilder = new AssemblyBuilder();
        }

        protected override void OnTerm()
        {
            _assemblyBuilder = null;
        }

        [Test]
        public void ShouldCreateJumpEntryFieldInTargetType()
        {
            var assembly = _assemblyBuilder.CreateAssembly(Guid.NewGuid().ToString(), AssemblyKind.Dll);
            var module = assembly.MainModule;

            var objectType = module.ImportType(typeof(object));
            var typeBuilder = new TypeBuilder();
            TypeDefinition targetType = typeBuilder.CreateType(Guid.NewGuid().ToString(), "Test", objectType, assembly);

            var entryDictionaryType = module.ImportType<Dictionary<int, int>>();
            var entryBuilder = new FieldBuilder();
            var targetField = entryBuilder.AddField(targetType, "___jumpEntries", entryDictionaryType);

            // The type should have a field that is a Dictionary<int, int>            
            Assert.IsNotNull(targetField);
            Assert.AreEqual(targetField.FieldType, entryDictionaryType);
        }

        [Test]
        public void ShouldPullAvailableDependenciesFromDependencyContainer()
        {
            var dependency = new Dependency(string.Empty, typeof(IVehicle));
            var dependencyList = new IDependency[] { dependency };
            var implementation = new Mock<IImplementation>();
            implementation.Expect(i => i.Emit(It.IsAny<IDependency>(), It.IsAny<MethodDefinition>()));

            var map = new Mock<IDependencyContainer>();
            map.Expect(m => m.Dependencies).Returns(dependencyList);
            map.Expect(m => m.GetImplementations(It.IsAny<IDependency>(), It.IsAny<bool>())).Returns(new IImplementation[] { implementation.Object });

            var compiler = new ContainerCompiler();
            compiler.Compile(map.Object);
            map.VerifyAll();
        }

        [Test]
        public void ShouldBeAbleToCreatePrivateStaticGetServiceHashCodeMethodForAGivenType()
        {
            var assembly = _assemblyBuilder.CreateAssembly(Guid.NewGuid().ToString(), AssemblyKind.Dll);
            var module = assembly.MainModule;

            TypeReference baseType = module.Import(typeof(object));

            var typeBuilder = new TypeBuilder();
            var newType = typeBuilder.CreateType("Test", "Test", baseType, assembly);

            var emitter = new ServiceHashEmitter();
            emitter.AddGetServiceHashMethodTo(newType, true);

            var result = (from MethodDefinition m in newType.Methods
                          where m.Name == "GetServiceHashCode"
                          select m).First();

            Assert.IsNotNull(result);

            // Load the assembly
            var loadedAssembly = assembly.ToAssembly();

            var containerType = loadedAssembly.GetTypes()[0];
            var targetMethod = containerType.GetMethod("GetServiceHashCode", BindingFlags.Public | BindingFlags.Static);

            Assert.IsNotNull(targetMethod);

            var serviceName = "Test";
            var serviceType = typeof(IVehicle);

            // The GetServiceHashCode method result should match the actual hash
            var expectedHash = serviceName.GetHashCode() ^ serviceType.GetHashCode();
            var actualHash = targetMethod.Invoke(null, new object[] { serviceType, serviceName });

            Assert.AreEqual(expectedHash, actualHash);
        }

        [Test]
        public void ShouldBeAbleToCreateInterfaceStub()
        {
            var interfaceType = typeof(IMicroContainer);

            ModuleDefinition module;
            TypeDefinition type;
            CreateStub("Test", "Test", interfaceType, out module, out type);

            TestStubbedInterfaceImplementation(module, type);
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void ShouldThrowNotImplementedExceptionWhenCallingContainsMethod()
        {
            ShouldThrowNotImplementedExceptionWith<IMicroContainer>(c => c.Contains(null, null));
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void ShouldThrowNotImplementedExceptionWhenCallingGetAllInstancesMethod()
        {
            ShouldThrowNotImplementedExceptionWith<IMicroContainer>(c => c.GetAllInstances(null));
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void ShouldThrowNotImplementedExceptionWhenCallingGetInstanceMethod()
        {
            ShouldThrowNotImplementedExceptionWith<IMicroContainer>(c => c.GetInstance(null, null));
        }

        [Test]
        public void ShouldCreateDLLAssemblyType()
        {
            string assemblyName = "TestAssembly";
            AssemblyDefinition result = _assemblyBuilder.CreateAssembly(assemblyName, AssemblyKind.Dll);

            Assert.AreEqual(result.Name.Name, assemblyName);
            Assert.AreEqual(result.Kind, AssemblyKind.Dll);
        }

        [Test]
        public void ShouldCreateContainerType()
        {
            var assembly = _assemblyBuilder.CreateAssembly(Guid.NewGuid().ToString(), AssemblyKind.Dll);
            var module = assembly.MainModule;

            TypeReference baseType = module.Import(typeof(object));
            var microContainerTypeRef = module.Import(typeof(IMicroContainer));

            TypeReference[] interfaces = new TypeReference[] { microContainerTypeRef };
            string typeName = "Hiro.MicroContainer";
            string namespaceName = "Hiro.Containers";

            var typeBuilder = new ContainerTypeBuilder();
            TypeDefinition result = typeBuilder.CreateType(typeName, namespaceName, baseType, assembly, interfaces);

            // Verify the type attributes
            Assert.IsTrue(result.IsAutoClass);
            Assert.IsTrue(result.IsClass);
            Assert.IsTrue(result.IsBeforeFieldInit);
            Assert.IsTrue(result.IsPublic);

            Assert.IsTrue(result.Interfaces.Contains(microContainerTypeRef));

            // Verify that the default constructor exists
            Assert.IsTrue(result.Constructors.Count > 0);
            Assert.IsTrue(result.Constructors[0].Parameters.Count == 0);
        }

        [Test]
        public void ShouldBeAbleToCreatePublicInstanceMethod()
        {
            TestCreatePublicMethod(false);
        }

        [Test]
        public void ShouldBeAbleToCreatePublicStaticMethod()
        {
            TestCreatePublicMethod(true);
        }

        [Test]
        public void ShouldProvideMethodOverrideForContainsMethod()
        {
            ShouldProvideMethodOverrideFor<IMicroContainer>("Contains");
        }

        [Test]
        public void ShouldProvideMethodOverrideForGetAllInstancesMethod()
        {
            ShouldProvideMethodOverrideFor<IMicroContainer>("GetAllInstances");
        }

        [Test]
        public void ShouldProvideMethodOverrideForGetInstanceMethod()
        {
            ShouldProvideMethodOverrideFor<IMicroContainer>("GetInstance");
        }

        [Test]
        public void ShouldBeAbleToCompileContainerUsingATypeWithMultipleConstructors()
        {
            var map = new DependencyMap();

            var dependency = new Dependency(string.Empty, typeof(IVehicle));
            var implementation = new TypeImplementation(typeof(Vehicle), map);

            map.AddService(dependency, implementation);
            map.AddService(new Dependency("", typeof(IPerson)), new TypeImplementation(typeof(Person), map));

            var container = Compile(map);
            var vehicle = (IVehicle)container.GetInstance(typeof(IVehicle), null);
            Assert.IsNotNull(vehicle);
            Assert.IsNotNull(vehicle.Driver);
        }

        [Test]
        public void ShouldBeAbleToCompileContainerWithParameterlessConstructor()
        {
            var targetConstructor = typeof(Vehicle).GetConstructor(new Type[0]);
            
            var dependency = new Dependency(string.Empty, typeof(IVehicle));
            var implementation = new ConstructorImplementation(targetConstructor);

            var map = new DependencyMap();
            map.AddService(dependency, implementation);

            var container = Compile(map);

            Assert.IsTrue(container.Contains(typeof(IVehicle), null));
            Assert.IsTrue(container.Contains(typeof(IVehicle), string.Empty));

            var result = container.GetInstance(typeof(IVehicle), null);
            Assert.IsNotNull(result);
        }

        private static IMicroContainer Compile(DependencyMap map)
        {
            var compiler = new ContainerCompiler();
            var assembly = compiler.Compile(map);

            var loadedAssembly = assembly.ToAssembly();

            Assert.IsNotNull(loadedAssembly);

            var targetType = (from t in loadedAssembly.GetTypes()
                              where typeof(IMicroContainer).IsAssignableFrom(t)
                              select t).First();

            var container = Activator.CreateInstance(targetType) as IMicroContainer;
            Assert.IsNotNull(container);

            return container;
        }

        private void ShouldProvideMethodOverrideFor<T>(string methodName)
            where T : class
        {
            MethodInfo targetMethod = typeof(T).GetMethod(methodName);

            var assembly = _assemblyBuilder.CreateAssembly("Test", AssemblyKind.Dll);
            var module = assembly.MainModule;

            var objectType = module.Import(typeof(object));
            var typeBuilder = new ContainerTypeBuilder();
            TypeDefinition hostType = typeBuilder.CreateType("Test", "Test", objectType, assembly);

            var overrider = new MethodOverrider();
            overrider.AddOverrideFor(targetMethod, hostType);

            // Search for the target method and make sure it matches the target method
            // signature
            var newMethod = (from MethodDefinition method in hostType.Methods
                             where method.Name == methodName
                             select method).First();

            // Match the parameters
            var index = 0;
            foreach (var param in targetMethod.GetParameters())
            {
                var parameterTypeRef = module.Import(param.ParameterType);
                var currentParameter = newMethod.Parameters[index];

                Assert.AreEqual(currentParameter.ParameterType, parameterTypeRef);
                index++;
            }

            // Match the return type
            var returnTypeRef = module.Import(targetMethod.ReturnType);

            if (!(returnTypeRef is GenericInstanceType))
            {
                Assert.AreEqual(returnTypeRef, newMethod.ReturnType.ReturnType);
            }
            else
            {
                var first = (GenericInstanceType)returnTypeRef;
                var second = (GenericInstanceType)newMethod.ReturnType.ReturnType;

                Assert.AreEqual(first.ElementType, second.ElementType);
            }

            // Verify the method attributes
            Assert.IsTrue(newMethod.IsVirtual);
            Assert.AreEqual(newMethod.IsPublic, targetMethod.IsPublic);
            Assert.AreEqual(newMethod.IsStatic, targetMethod.IsStatic);
            Assert.AreEqual(newMethod.IsHideBySig, targetMethod.IsHideBySig);
        }

        private void TestCreatePublicMethod(bool isStatic)
        {
            var typeBuilder = new ContainerTypeBuilder();
            var methodBuilder = new MethodBuilder();
            MethodBuilderOptions options = new MethodBuilderOptions();

            var assembly = _assemblyBuilder.CreateAssembly("SomeAssembly", AssemblyKind.Dll);
            var module = assembly.MainModule;

            var objectType = module.Import(typeof(object));
            var targetType = typeBuilder.CreateType("SomeType", "SomeNamespace", objectType, assembly);

            var methodName = "SomeMethod";

            options.MethodName = methodName;
            options.HostType = targetType;
            options.SetMethodParameters(typeof(int), typeof(int));
            options.ReturnType = typeof(void);

            options.IsPublic = true;
            options.IsStatic = isStatic;

            MethodDefinition result = methodBuilder.CreateMethod(options);

            // Verify the method attributes
            Assert.IsTrue(result.IsPublic);
            Assert.IsFalse(result.IsAbstract);
            if (!isStatic)
            {
                Assert.IsTrue(result.HasThis);
                Assert.IsTrue(result.IsVirtual);
            }
            else
            {
                Assert.IsTrue(result.IsStatic);
            }

            // Check the method signature
            Assert.IsTrue(result.Parameters.Count == 2);

            var integerType = module.Import(typeof(int));

            foreach (ParameterDefinition param in result.Parameters)
            {
                Assert.IsTrue(param.ParameterType == integerType);
            }
        }
        private void ShouldThrowNotImplementedExceptionWith<T>(Action<T> actionThatShouldTriggerException)
        {
            var interfaceType = typeof(T);

            ModuleDefinition module;
            TypeDefinition type;
            CreateStub("Test", "Test", interfaceType, out module, out type);

            var assembly = module.Assembly;
            var loadedAssembly = assembly.ToAssembly();

            var firstType = loadedAssembly.GetTypes()[0];
            Assert.IsNotNull(firstType);

            var instance = (T)Activator.CreateInstance(firstType);
            actionThatShouldTriggerException(instance);
        }

        private void CreateStub(string typeName, string assemblyName, Type interfaceType, out ModuleDefinition module, out TypeDefinition type)
        {
            var assembly = _assemblyBuilder.CreateAssembly(assemblyName, AssemblyKind.Dll);
            module = assembly.MainModule;

            var objectType = module.Import(typeof(object));

            var typeBuilder = new TypeBuilder();
            type = typeBuilder.CreateType(typeName, "Test", objectType, assembly);

            var stubBuilder = new InterfaceStubBuilder();
            stubBuilder.AddStubImplementationFor(interfaceType, type);

            var interfaceTypeRef = module.Import(interfaceType);

            Assert.IsTrue(type.Interfaces.Contains(interfaceTypeRef));
        }

        private static void TestStubbedInterfaceImplementation(ModuleDefinition module, TypeDefinition type)
        {
            var notImplementedCtor = module.ImportConstructor<NotImplementedException>();

            // All stub methods must throw a NotImplementedException
            foreach (MethodDefinition method in type.Methods)
            {
                var body = method.Body;
                var instructions = body.Instructions;

                // Define the expected constructors
                var IL = body.CilWorker;
                var expectedInstructions = new Queue<Instruction>();
                expectedInstructions.Enqueue(IL.Create(OpCodes.Newobj, notImplementedCtor));
                expectedInstructions.Enqueue(IL.Create(OpCodes.Throw));

                Assert.AreEqual(expectedInstructions.Count, instructions.Count);

                foreach (Instruction instruction in instructions)
                {
                    var expectedInstruction = expectedInstructions.Dequeue();

                    Assert.AreEqual(expectedInstruction.OpCode, instruction.OpCode);
                    Assert.AreEqual(expectedInstruction.Operand, instruction.Operand);
                }
            }
        }
    }
}
