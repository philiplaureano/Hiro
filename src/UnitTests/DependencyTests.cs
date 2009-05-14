using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Implementations;
using Hiro.Interfaces;
using Hiro.UnitTests.SampleDomain;
using Moq;
using NUnit.Framework;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class DependencyTests : BaseFixture
    {
        [Test]
        public void ShouldCallImplementationInjectorIfItExists()
        {
            var injector = new Mock<IImplementationInjector>();
            var map = new DependencyMap();
            map.Injector = injector.Object;

            injector.Expect(i => i.Inject(It.IsAny<IDependency>(), It.IsAny<IImplementation>())).Returns(new TransientType(typeof(Vehicle), map));

            map.AddService<IVehicle, Vehicle>();
            injector.VerifyAll();
        }

        [Test]
        public void ShouldBeEqualIfServiceNameAndServiceTypeAreTheSame()
        {
            var first = new Dependency(typeof(IPerson), string.Empty);
            var second = new Dependency(typeof(IPerson), string.Empty);

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
        public void ShouldBeAbleToRegisterAnonymousSingletonServicesWithDependencyMap()
        {
            var dependencyMap = new DependencyMap();
            dependencyMap.AddSingletonService<IVehicle, Vehicle>();

            Assert.IsTrue(dependencyMap.Contains(new Dependency(typeof(IVehicle))));
            Assert.IsTrue(dependencyMap.Contains(typeof(IVehicle)));
        }

        [Test]
        public void ShouldBeAbleToRegisterNamedSingletonServicesWithDependencyMapUsingGenerics()
        {
            var serviceName = "MyService";
            var dependencyMap = new DependencyMap();
            dependencyMap.AddSingletonService<IVehicle, Vehicle>(serviceName);
            Assert.IsTrue(dependencyMap.Contains(typeof(IVehicle), serviceName));
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
            var dependency = new Dependency(typeof(IVehicle), string.Empty);
            var constructorImplementation = new ConstructorCall(ctor);

            var dependencyMap = new DependencyMap();
            dependencyMap.AddService(dependency, constructorImplementation);
            Assert.IsTrue(dependencyMap.Contains(dependency));
        }

        [Test]
        public void ShouldReturnImplementationsFromDependencyMapFromImplementationsThatHaveNoMissingDependencies()
        {
            var map = new DependencyMap();
            var dependency = new Dependency(typeof(IVehicle), string.Empty);
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
