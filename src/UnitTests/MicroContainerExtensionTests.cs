using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Hiro.Containers;
using Hiro.Implementations;
using Moq;
using NUnit.Framework;
using Hiro.UnitTests.SampleDomain;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class MicroContainerExtensionTests : BaseFixture
    {
        [Test]
        public void ShouldBeAbleToUseGenericsToGetAnonymousServiceInstances()
        {
            var map = new DependencyMap();
            map.AddService(typeof(IPerson), typeof(Person));

            var container = map.CreateContainer();
            IPerson person = container.GetInstance<IPerson>();

            Assert.IsNotNull(person);
        }

        [Test]
        public void ShouldBeAbleToUseGenericsToGetNamedServiceInstances()
        {
            var map = new DependencyMap();
            map.AddService("Person", typeof(IPerson), typeof(Person));

            var container = map.CreateContainer();
            IPerson person = container.GetInstance<IPerson>("Person");

            Assert.IsNotNull(person);
            Assert.IsNull(container.GetInstance<IPerson>());
        }
        [Test]
        public void ShouldBeAbleToAddExistingServiceInstanceToContainer()
        {
            var map = new DependencyMap();
            var person = new Person();

            // Create a blank container
            var container = map.CreateContainer();

            container.AddService<IPerson>(null, person);

            // The container should return the added instance
            var result = container.GetInstance<IPerson>();
            Assert.AreSame(person, result);
        }

        [Test]
        public void ShouldBeAbleToAddDeferredServiceToContainer()
        {            
            var map = new DependencyMap();            
            map.AddDeferredService(typeof(IPerson));
            
            map.Injector = new PropertyInjector();
            map.AddService(typeof(Truck), typeof(Truck));

            var mockPerson = new Mock<IPerson>();
            var container = map.CreateContainer();
            container.AddService(mockPerson.Object);

            // The container must instantiate the mock person type
            var person = container.GetInstance<IPerson>();
            Assert.AreSame(mockPerson.Object, person);
            Assert.IsNotNull(person);

            // Make sure the person instance is injected into
            // the target property
            var truck = container.GetInstance<Truck>();
            Assert.IsNotNull(truck);
            Assert.AreSame(truck.Driver, mockPerson.Object);
        }

        [Test]
        [ExpectedException(typeof(ServiceNotFoundException))]
        public void ShouldThrowServiceNotFoundExceptionIfDeferredServiceIsNotAvailableAtRuntime()
        {
            var map = new DependencyMap();
            map.AddDeferredService(typeof(IPerson));

            var container = map.CreateContainer();

            // The exception should be thrown on this line of code
            var person = container.GetInstance<IPerson>();

            // This line of code should never be executed
            person.ToString();
        }
    }
}
