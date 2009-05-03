using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var injector = new PropertyInjector(new TransientType(typeof(Vehicle), map));
            map.AddService(dependency, injector);

            map.AddService(typeof(IPerson), typeof(Person));

            var container = map.CreateContainer();

            var result = (IVehicle)container.GetInstance(typeof(IVehicle), null);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Driver);
            Assert.IsTrue(result.Driver is Person);
        }
    }
}
