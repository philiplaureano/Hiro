using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mono.Cecil;
using Hiro.Compilers;
using Hiro.Containers;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class CompilerTests : BaseFixture
    {
        [Test]
        public void ShouldCreateDLLAssemblyType()
        {
            string assemblyName = "TestAssembly";
            var assemblyBuilder = new AssemblyBuilder();
            AssemblyDefinition result = assemblyBuilder.CreateAssembly(assemblyName, AssemblyKind.Dll);

            Assert.AreEqual(result.Name.Name, assemblyName);
            Assert.AreEqual(result.Kind, AssemblyKind.Dll);
        }

        [Test]
        public void ShouldCreateContainerType()
        {
            var builder = new AssemblyBuilder();
            var assembly = builder.CreateAssembly(Guid.NewGuid().ToString(), AssemblyKind.Dll);
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
    }
}
