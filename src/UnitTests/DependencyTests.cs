using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Hiro.UnitTests.SampleDomain;
using System.Reflection;
using Moq;
using Hiro.Interfaces;
using Hiro.Implementations;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class DependencyTests : BaseFixture
    {
        [Test]
        public void ShouldBeEqualIfServiceNameAndServiceTypeAreTheSame()
        {
            var first = new Dependency(string.Empty, typeof(IPerson));
            var second = new Dependency(string.Empty, typeof(IPerson));

            Assert.AreEqual(first, second);
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        }

        [Test]
        public void ShouldBeAbleToRegisterAnonymousServicesWithDependencyMapUsingGenerics()
        {
            var dependencyMap = new DependencyMap();
            dependencyMap.AddService<IVehicle, Vehicle>();

            Assert.IsTrue(dependencyMap.Contains(new Dependency(typeof(IVehicle))));
            Assert.IsTrue(dependencyMap.Contains(typeof(IVehicle)));
        }

        [Test]
        public void ShouldBeAbleToRegisterNamedServicesWithDependencyMap()
        {
            var serviceName = "MyService";
            var dependencyMap = new DependencyMap();
            dependencyMap.AddService<IVehicle, Vehicle>(serviceName);
            Assert.IsTrue(dependencyMap.Contains(typeof(IVehicle), serviceName));
        }

        [Test]
        public void ShouldBeAbleToRegisterAnonymousServicesWithDependencyMap()
        {
            var dependencyMap = new DependencyMap();
            dependencyMap.AddService(typeof(IVehicle), typeof(Vehicle));

            Assert.IsTrue(dependencyMap.Contains(new Dependency(typeof(IVehicle))));
            Assert.IsTrue(dependencyMap.Contains(typeof(IVehicle)));
        }

        [Test]
        public void ShouldBeAbleToRegisterNamedServicesWithDependencyMapUsingGenerics()
        {
            var serviceName = "MyService";
            var dependencyMap = new DependencyMap();
            dependencyMap.AddService<IVehicle, Vehicle>(serviceName);
            Assert.IsTrue(dependencyMap.Contains(typeof(IVehicle), serviceName));
        }

        [Test]
        public void ShouldBeAbleToAddItemsToDependencyMap()
        {
            var ctor = typeof(Vehicle).GetConstructor(new Type[0]);
            var dependency = new Dependency(string.Empty, typeof(IVehicle));
            var constructorImplementation = new ConstructorImplementation(ctor);

            var dependencyMap = new DependencyMap();
            dependencyMap.AddService(dependency, constructorImplementation);
            Assert.IsTrue(dependencyMap.Contains(dependency));
        }

        [Test]
        public void ShouldReturnImplementationsFromDependencyMapFromImplementationsThatHaveNoMissingDependencies()
        {
            var map = new DependencyMap();            
            var dependency = new Dependency(string.Empty, typeof(IVehicle));
            var implementation = new Mock<IImplementation>();
            implementation.Expect(impl => impl.GetMissingDependencies(map)).Returns(new IDependency[0]);
            
            bool addIncompleteImplementations = false;
            map.AddService(dependency, implementation.Object);
            var results = map.GetImplementations(dependency, addIncompleteImplementations);

            Assert.IsTrue(results.Count() > 0);
            Assert.IsTrue(results.Contains(implementation.Object));

            implementation.VerifyAll();
        }

        [Test]
        public void ShouldBeAbleToGetCurrentListOfDependencies()
        {
            var map = new DependencyMap();
            for (int i = 0; i < 10; i++)
            {
                var dependency = new Mock<IDependency>();
                var implementation = new Mock<IImplementation>();

                map.AddService(dependency.Object, implementation.Object);
                Assert.IsTrue(map.Dependencies.Contains(dependency.Object));
            }
        }
    }
}
