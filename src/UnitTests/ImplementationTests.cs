using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Moq;
using Hiro.UnitTests.SampleDomain;
using System.Reflection;
using Hiro.Interfaces;
using Hiro.Implementations;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class ImplementationTests : BaseFixture
    {
        [Test]
        public void ShouldListMissingDependencies()
        {
            var dependencyMap = new Mock<IDependencyContainer>();
            dependencyMap.Expect(m => m.Contains(It.IsAny<IDependency>())).Returns(false);

            TestMissingDependencies(dependencyMap, 1);
        }

        [Test]
        public void ShouldListNoMissingDependenciesIfDependencyMapReturnsTrue()
        {
            var dependencyMap = new Mock<IDependencyContainer>();
            dependencyMap.Expect(m => m.Contains(It.IsAny<IDependency>())).Returns(true);
            dependencyMap.Expect(m => m.Dependencies).Returns(new IDependency[0]);

            TestMissingDependencies(dependencyMap, 0);
        }

        [Test]
        public void ShouldBeAbleToListRequiredDependencies()
        {
            var constructorResolver = new Mock<IDependencyResolver<ConstructorInfo>>();
            var dependency = new Dependency(string.Empty, typeof(IPerson));
            
            var constructor = typeof(Vehicle).GetConstructor(new Type[] { typeof(IPerson) });
            var constructorImplementation = new ConstructorImplementation(constructor, constructorResolver.Object);

            constructorResolver.Expect(resolver => resolver.GetDependenciesFrom(constructor)).Returns(new[] { dependency});

            IEnumerable<IDependency> results = constructorImplementation.GetRequiredDependencies();
            
            Assert.IsTrue(results.Count() > 0);
            Assert.IsTrue(results.Contains(dependency));
        }
        
        private static void TestMissingDependencies(Mock<IDependencyContainer> dependencyMap, int expectedResultCount)
        {
            var resolver = new Mock<IDependencyResolver<ConstructorInfo>>();
            var constructor = typeof(Vehicle).GetConstructor(new Type[] { typeof(IPerson) });
            var dependencies = new IDependency[] { new Dependency("", typeof(IPerson)) };

            resolver.Expect(r => r.GetDependenciesFrom(constructor)).Returns(dependencies);

            var implementation = new ConstructorImplementation(constructor, resolver.Object);
            var missingDependencies = implementation.GetMissingDependencies(dependencyMap.Object);

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
