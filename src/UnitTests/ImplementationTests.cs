using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Moq;
using Hiro.UnitTests.SampleDomain;
using System.Reflection;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class ImplementationTests : BaseFixture
    {
        [Test]
        public void ShouldListMissingDependencies()
        {
            var dependencyMap = new Mock<IDependencyMap>();
            dependencyMap.Expect(m => m.Contains(It.IsAny<IDependency>())).Returns(false);

            TestMissingDependencies(dependencyMap, 1);
        }

        [Test]
        public void ShouldListNoMissingDependenciesIfDependencyMapReturnsTrue()
        {
            var dependencyMap = new Mock<IDependencyMap>();
            dependencyMap.Expect(m => m.Contains(It.IsAny<IDependency>())).Returns(true);

            TestMissingDependencies(dependencyMap, 0);
        }

        private static void TestMissingDependencies(Mock<IDependencyMap> dependencyMap, int expectedResultCount)
        {
            var resolver = new Mock<IDependencyResolver<ConstructorInfo>>();
            var constructor = typeof(Vehicle).GetConstructor(new Type[] { typeof(IPerson) });
            var dependencies = new IDependency[] { new Dependency("", typeof(IPerson)) };

            resolver.Expect(r => r.GetDependenciesFrom(constructor)).Returns(dependencies);

            var implementation = new Implementation<ConstructorInfo>(constructor);
            var missingDependencies = implementation.GetMissingDependencies(dependencyMap.Object, resolver.Object);

            Assert.IsTrue(missingDependencies.Count() == expectedResultCount);

            if (missingDependencies.Count() == 0)
                return;

            var result = missingDependencies.First();

            Assert.AreEqual(string.Empty, result.ServiceName);
            Assert.AreEqual(typeof(IPerson), result.ServiceType);

            resolver.VerifyAll();
            dependencyMap.VerifyAll();
        }
    }
}
