using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Compilers;
using Hiro.Containers;
using Hiro.Implementations;
using Hiro.Interfaces;
using Hiro.Resolvers;
using Hiro.UnitTests.SampleDomain;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Moq;
using NUnit.Framework;
using Hiro.Loaders;

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
        public void ShouldNotCauseStackOverflowExceptionWhenCallingGetAllInstancesOnTheNextContainerAndNextContainerIsSelf()
        {
            var map = new DependencyMap();
            var container = map.CreateContainer();
            container.NextContainer = container;

            var result = container.GetAllInstances(typeof(int));
            var resultList = new List<object>(result);
            Assert.IsTrue(resultList.Count == 0);
        }

        [Test]
        public void ShouldNotCauseStackOverflowExceptionWhenCallingGetInstanceOnTheNextContainerAndNextContainerIsSelf()
        {
            var map = new DependencyMap();
            var container = map.CreateContainer();
            container.NextContainer = container;

            var result = container.GetInstance(typeof(int), "abcdefg");
            Assert.IsNull(result);
        }

        [Test]
        public void ShouldNotCauseStackOverflowExceptionWhenCallingContainsOnTheNextContainerAndNextContainerIsSelf()
        {
            var map = new DependencyMap();
            var container = map.CreateContainer();
            container.NextContainer = container;

            var result = container.Contains(typeof(int), "abcdefg");
            Assert.IsFalse(result);
        }

        [Test]
        public void ShouldCallGetAllInstancesMethodOnNextContainer()
        {
            var map = new DependencyMap();
            var container = map.CreateContainer();

            Assert.IsNotNull(container);

            var mockContainer = new Mock<IMicroContainer>();
            mockContainer.Expect(m => m.GetAllInstances(typeof(int))).Returns(new object[] { 42 });

            container.NextContainer = mockContainer.Object;

            var results = container.GetAllInstances(typeof(int));

            mockContainer.VerifyAll();

            var resultList = new List<object>(results);
            Assert.IsTrue(resultList.Contains(42));
        }

        [Test]
        public void ShouldCallGetInstanceMethodOnNextContainer()
        {
            var map = new DependencyMap();
            var container = map.CreateContainer();

            Assert.IsNotNull(container);

            var mockContainer = new Mock<IMicroContainer>();
            mockContainer.Expect(m => m.GetInstance(It.IsAny<Type>(), It.IsAny<string>())).Returns(42);
            container.NextContainer = mockContainer.Object;

            Assert.AreSame(container.NextContainer, mockContainer.Object);

            var result = container.GetInstance(typeof(int), "abcdefg");

            Assert.AreEqual(42, result);
            mockContainer.VerifyAll();
        }

        [Test]
        public void ShouldCallContainsMethodOnNextContainer()
        {
            var map = new DependencyMap();
            var container = map.CreateContainer();

            Assert.IsNotNull(container);

            var mockContainer = new Mock<IMicroContainer>();
            mockContainer.Expect(m => m.Contains(It.IsAny<Type>(), It.IsAny<string>())).Returns(true);
            container.NextContainer = mockContainer.Object;

            // The Contains() call on the created container should trigger
            // the contains method on the mock container
            var result = container.Contains(typeof(int), "abcd");

            Assert.IsTrue(result);
            mockContainer.VerifyAll();
        }

        [Test]
        public void ShouldBeAbleToAssignTheNextContainerToACompiledContainer()
        {
            var map = new DependencyMap();
            var container = map.CreateContainer();

            Assert.IsNotNull(container);

            var mockContainer = new Mock<IMicroContainer>();
            container.NextContainer = mockContainer.Object;

            Assert.AreSame(container.NextContainer, mockContainer.Object);
        }

        [Test]
        public void ShouldBeAbleToCompileContainerFromDependencyMap()
        {
            var map = new DependencyMap();
            map.AddService(typeof(IPerson), typeof(Person));

            var container = map.CreateContainer();
            var result = container.GetInstance(typeof(IPerson), null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result is Person);
        }

        [Test]
        public void ShouldBeAbleToAddAnonymousServiceToMapUsingExtensionMethod()
        {
            var map = new DependencyMap();
            map.AddSingletonService(typeof(IPerson), typeof(Person));

            var container = map.CreateContainer();
            var result = container.GetInstance(typeof(IPerson), null);

            Assert.IsNotNull(result);
            Assert.IsTrue(result is Person);
        }

        [Test]
        public void ShouldBeAbleToReturnTheSameSingletonInstance()
        {
            var map = new DependencyMap();
            map.AddSingletonService(typeof(IPerson), typeof(Person));

            var container = map.CreateContainer();
            var result = container.GetInstance(typeof(IPerson), null);
            for (var i = 0; i < 100; i++)
            {
                var currentResult = container.GetInstance(typeof(IPerson), null);
                Assert.AreSame(result, currentResult);
            }
        }

        [Test]
        public void ShouldBeAbleToAddNamedServiceToMapUsingExtensionMethod()
        {
            var map = new DependencyMap();
            map.AddSingletonService("SomePerson", typeof(IPerson), typeof(Person));

            var container = map.CreateContainer();
            var result = container.GetInstance(typeof(IPerson), "SomePerson");

            Assert.IsNotNull(result);
            Assert.IsTrue(result is Person);
        }

        [Test]
        public void ShouldCreateJumpEntryFieldInTargetType()
        {
            var assembly = _assemblyBuilder.CreateAssembly(Guid.NewGuid().ToString(), ModuleKind.Dll);
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
            var dependency = new Dependency(typeof(IVehicle), string.Empty);
            var dependencyList = new IDependency[] { dependency };
            var implementation = new Mock<IImplementation>();
            implementation.Expect(i => i.Emit(It.IsAny<IDependency>(), It.IsAny<IDictionary<IDependency, IImplementation>>(), It.IsAny<MethodDefinition>()));

            var map = new Mock<IDependencyContainer>();
            map.Expect(m => m.Dependencies).Returns(dependencyList);
            map.Expect(m => m.GetImplementations(It.IsAny<IDependency>(), It.IsAny<bool>())).Returns(new IImplementation[] { implementation.Object });

            var compiler = new ContainerCompiler();
            compiler.Compile("MicroContainer", "Hiro.Containers", "Hiro.CompiledContainers", map.Object);
            map.VerifyAll();
        }

        [Test]
        public void ShouldBeAbleToCreatePrivateStaticGetServiceHashCodeMethodForAGivenType()
        {
            var assembly = _assemblyBuilder.CreateAssembly(Guid.NewGuid().ToString(), ModuleKind.Dll);
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
            AssemblyDefinition result = _assemblyBuilder.CreateAssembly(assemblyName, ModuleKind.Dll);

            Assert.AreEqual(result.Name.Name, assemblyName);
            Assert.AreEqual(result.MainModule.Kind, ModuleKind.Dll);
        }

        [Test]
        public void ShouldCreateContainerType()
        {
            var assembly = _assemblyBuilder.CreateAssembly(Guid.NewGuid().ToString(), ModuleKind.Dll);
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

            Assert.IsTrue(result.Implements(microContainerTypeRef));

            // Verify that the default constructor exists
            var constructor = result.GetDefaultConstructor();
            Assert.IsNotNull(constructor);
            Assert.IsTrue(constructor.Parameters.Count == 0);
        }

        [Test]
        public void ShouldBeAbleToCreateTheSameContainerTypeFromASingleDependencyMap()
        {
            var map = new DependencyMap();
            map.AddService(typeof(IPerson), typeof(Person));


            var firstContainer = map.CreateContainer();
            var secondContainer = map.CreateContainer();

            Assert.AreEqual(firstContainer.GetType(), secondContainer.GetType());
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
        public void ShouldBeAbleToCreateServiceAsSingleton()
        {
            var map = new DependencyMap();
            var dependency = new Dependency(typeof(IVehicle));

            map.AddService(dependency, new SingletonType(typeof(Vehicle), map, new ConstructorResolver()));

            var container = Compile(map);
            var first = container.GetInstance(typeof(IVehicle), null);
            Assert.IsNotNull(first);

            // The GetInstance call must return the same instance
            for (int i = 0; i < 10; i++)
            {
                var currentInstance = container.GetInstance(typeof(IVehicle), null);
                Assert.AreSame(first, currentInstance);
            }
        }

        [Test]
        public void ShouldBeAbleToGetAllInstancesOfATypeFromACompiledContainer()
        {
            var map = new DependencyMap();

            map.AddService(typeof(IVehicle), typeof(Vehicle));
            map.AddService("Truck", typeof(IVehicle), typeof(Truck));

            var compiler = new ContainerCompiler();
            var assembly = compiler.Compile("MicroContainer", "Hiro.Containers", "Hiro.CompiledContainers", map);

            var container = Compile(map);
            var instances = container.GetAllInstances(typeof(IVehicle));

            Assert.IsNotNull(instances);
            Assert.IsTrue(instances.Count() == 2);

            var items = instances.ToArray();
            Assert.IsTrue(items[0] is Vehicle);
            Assert.IsTrue(items[1] is Truck);
        }
        [Test]
        public void ShouldBeAbleToCompileContainerUsingATypeWithMultipleConstructors()
        {
            var map = new DependencyMap();

            map.AddService(typeof(IVehicle), typeof(Vehicle));
            map.AddService(typeof(IPerson), typeof(Person));

            var container = Compile(map);
            var result = container.GetInstance(typeof(IVehicle), null);

            var vehicle = (IVehicle)result;
            Assert.IsNotNull(vehicle);
            Assert.IsNotNull(vehicle.Driver);
        }

        [Test]
        public void ShouldBeAbleToCompileContainerWithParameterlessConstructor()
        {
            var targetConstructor = typeof(Vehicle).GetConstructor(new Type[0]);

            var dependency = new Dependency(typeof(IVehicle), string.Empty);
            var implementation = new ConstructorCall(targetConstructor);

            var map = new DependencyMap();
            map.AddService(dependency, implementation);

            var container = Compile(map);

            Assert.IsTrue(container.Contains(typeof(IVehicle), string.Empty));

            var result = container.GetInstance(typeof(IVehicle), string.Empty);
            Assert.IsNotNull(result);
        }

        [Test]
        public void ShouldIntroduceContainerInstanceToTypesThatHaveTheMicroContainerDependencyInTheirConstructors()
        {
            var map = new DependencyMap();
            map.AddService(typeof(SampleContainerAwareType), typeof(SampleContainerAwareType));

            var container = map.CreateContainer();
            var instance = container.GetInstance<SampleContainerAwareType>();
            Assert.IsNotNull(instance);

            Assert.IsNotNull(instance.Container);
            Assert.AreSame(container, instance.Container);
        }

        [Test]
        public void ShouldUseFactoryToCreateTypesIfFactoryTypeIsAvailable()
        {
            var loader = new DependencyMapLoader();
            var map = loader.LoadFromBaseDirectory("SampleAssembly.dll");

            var container = map.CreateContainer();
            var result = container.GetInstance<object>("Sample");

            Assert.AreEqual(42, result);
        }

        [Test]
        public void ShouldUseDefaultAnonymousFactoryToCreateTypesIfFactoryTypeIsAvailable()
        {
            var loader = new DependencyMapLoader();
            var map = loader.LoadFromBaseDirectory("SampleAssembly.dll");

            var container = map.CreateContainer();
            var result = container.GetInstance<object>();

            Assert.AreEqual(42, result);
        }

        [Test]
        public void ShouldCallInitializeOnTransientTypeThatImplementsIInitialize()
        {
            var map = new DependencyMap();
            map.AddService<IInitialize, SampleInitialize>();

            var container = map.CreateContainer();
            var result = (SampleInitialize)container.GetInstance<IInitialize>();

            Assert.AreSame(container, result.Container);
            Assert.IsTrue(result.NumberOfTimesInitialized == 1);
        }


        [Test]
        public void ShouldCallInitializeOnTransientTypeThatImplementsIInitializeWhenCallingGetAllInstances()
        {
            var map = new DependencyMap();
            map.AddService<IInitialize, SampleInitialize>();

            var container = map.CreateContainer();
            var result = (SampleInitialize)container.GetAllInstances(typeof(IInitialize)).Cast<IInitialize>().First();

            Assert.AreSame(container, result.Container);
            Assert.IsTrue(result.NumberOfTimesInitialized == 1);
        }

        [Test]
        public void ShouldCallInitializeOnSingletonTypeThatImplementsIInitializeOnceAndOnlyOnce()
        {
            var map = new DependencyMap();
            map.AddSingletonService<IInitialize, SampleInitialize>();

            var container = map.CreateContainer();
            var result = (SampleInitialize)container.GetInstance<IInitialize>();
            for (var i = 0; i < 100; i++)
            {
                result = (SampleInitialize)container.GetInstance<IInitialize>();
            }

            Assert.AreSame(container, result.Container);
            Assert.IsTrue(result.NumberOfTimesInitialized == 1);
        }

        [Test]
        public void ShouldCallContainerPluginOnceContainerIsInstantiated()
        {
            var map = new DependencyMap();
            map.AddService<IContainerPlugin, SamplePlugin>();

            var container = map.CreateContainer();
            Assert.IsTrue(SamplePlugin.HasBeenCalled);
        }

        [Test]
        public void ShouldUseFirstNamedServiceInstanceIfNoDefaultServiceIsAvailable()
        {
            var map = new DependencyMap();
            map.AddService<Garage, Garage>();
            map.AddService<IVehicle, Truck>("Truck");

            var container = map.CreateContainer();

            var garage = container.GetInstance<Garage>();
            Assert.IsNotNull(garage.Vehicle);
            Assert.IsInstanceOfType(typeof(Truck), garage.Vehicle);
        }

        [Test]
        public void ShouldUseConstructorParameterNameToInjectNamedServiceInstanceIfTheNamedServiceExists()
        {
            var map = new DependencyMap();
            map.AddService<IVehicle, Vehicle>("Vehicle");
            map.AddService<IVehicle, Truck>("Truck");
            map.AddService<Garage, Garage>();

            var container = map.CreateContainer();

            var garage = container.GetInstance<Garage>();
            Assert.IsNotNull(garage.Vehicle);
            Assert.IsInstanceOfType(typeof(Vehicle), garage.Vehicle);
        }

        private static IMicroContainer Compile(DependencyMap map)
        {
            var compiler = new ContainerCompiler();
            var assembly = compiler.Compile("MicroContainer", "Hiro.Containers", "Hiro.CompiledContainers", map);

            try
            {
                if (File.Exists("output.dll"))
                    File.Delete("output.dll");

                assembly.Write("output.dll");
            }
            catch
            {
                // Do nothing
            }

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

            var assembly = _assemblyBuilder.CreateAssembly("Test", ModuleKind.Dll);
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

                Assert.IsTrue(currentParameter.ParameterType.IsEquivalentTo(parameterTypeRef));
                index++;
            }

            // Match the return type
            var returnTypeRef = module.Import(targetMethod.ReturnType);

            if (!(returnTypeRef is GenericInstanceType))
            {
                Assert.IsTrue(newMethod.ReturnType.IsEquivalentTo(returnTypeRef));
            }
            else
            {
                var first = (GenericInstanceType)returnTypeRef;
                var second = (GenericInstanceType)newMethod.ReturnType;

                Assert.IsTrue(first.ElementType.IsEquivalentTo(second.ElementType));
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

            var assembly = _assemblyBuilder.CreateAssembly("SomeAssembly", ModuleKind.Dll);
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
                Assert.IsTrue(param.ParameterType.IsEquivalentTo(integerType));
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
            var assembly = _assemblyBuilder.CreateAssembly(assemblyName, ModuleKind.Dll);
            module = assembly.MainModule;

            var objectType = module.Import(typeof(object));

            var typeBuilder = new TypeBuilder();
            type = typeBuilder.CreateType(typeName, "Test", objectType, assembly);

            var stubBuilder = new InterfaceStubBuilder();
            stubBuilder.AddStubImplementationFor(interfaceType, type);

            var interfaceTypeRef = module.Import(interfaceType);

            Assert.IsTrue(type.Implements(interfaceTypeRef));
        }

        private static void TestStubbedInterfaceImplementation(ModuleDefinition module, TypeDefinition type)
        {
            var notImplementedCtor = module.ImportConstructor<NotImplementedException>();

            // All stub methods must throw a NotImplementedException
            foreach (MethodDefinition method in type.Methods)
            {
                if (method.IsConstructor)
                    continue;

                var body = method.Body;
                var instructions = body.Instructions;

                Assert.AreEqual(2, instructions.Count);

                Assert.AreEqual(OpCodes.Newobj, instructions[0].OpCode);
                Assert.IsTrue(MethodsAreEqual(notImplementedCtor, (MethodReference)instructions[0].Operand));
                Assert.AreEqual(OpCodes.Throw, instructions[1].OpCode);
            }
        }

        private static bool MethodsAreEqual(MethodReference expectedMethod, MethodReference method)
        {
            if (expectedMethod.FullName != method.FullName)
                return false;

            return expectedMethod.DeclaringType.IsEquivalentTo(method.DeclaringType);
        }
    }
}
