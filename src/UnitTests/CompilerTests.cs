using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mono.Cecil;
using Hiro.Compilers;
using Hiro.Containers;
using System.Reflection;

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

            var typeBuilder = new TypeBuilder();
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

        private void ShouldProvideMethodOverrideFor<T>(string methodName)
            where T : class
        {
            MethodInfo targetMethod = typeof(T).GetMethod(methodName);

            var assembly = _assemblyBuilder.CreateAssembly("Test", AssemblyKind.Dll);
            var module = assembly.MainModule;

            var objectType = module.Import(typeof(object));
            var typeBuilder = new TypeBuilder();
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
            var typeBuilder = new TypeBuilder();
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
            Assert.AreEqual(result.HasThis, !isStatic);
            Assert.IsTrue(result.IsVirtual);
            Assert.IsFalse(result.IsAbstract);

            // Check the method signature
            Assert.IsTrue(result.Parameters.Count == 2);

            var integerType = module.Import(typeof(int));

            foreach (ParameterDefinition param in result.Parameters)
            {
                Assert.IsTrue(param.ParameterType == integerType);
            }
        }
    }
}
