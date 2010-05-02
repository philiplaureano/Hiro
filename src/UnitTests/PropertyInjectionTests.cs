using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Containers;
using Hiro.Resolvers;
using NUnit.Framework;
using Hiro.UnitTests.SampleDomain;
using Hiro.Implementations;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class PropertyInjectionTests : BaseFixture 
    {
        [Test]
        public void ShouldInjectDefaultServiceImplementationIntoTargetProperty()
        {
            var map = new DependencyMap();

            var dependency = new Dependency(typeof(IVehicle));
            var injector = new PropertyInjectionCall(new TransientType(typeof(Vehicle), map, new ConstructorResolver()));
            map.AddService(dependency, injector);

            map.AddService(typeof(IPerson), typeof(Person));

            var container = map.CreateContainer();

            var result = (IVehicle)container.GetInstance(typeof(IVehicle), null);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Driver);
            Assert.IsTrue(result.Driver is Person);
        }

        [Test]
        public void ShouldInjectPropertyIfDependencyMapHasAPropertyInjectorAssignedToTheInjectorProperty()
        {
            var map = new DependencyMap();
            map.Injector = new PropertyInjector();

            map.AddService<IVehicle, Vehicle>();
            map.AddService<IPerson, Person>();

            var container = map.CreateContainer();

            var result = (IVehicle)container.GetInstance(typeof(IVehicle), null);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Driver);
            Assert.IsTrue(result.Driver is Person);
        }
    }
}
