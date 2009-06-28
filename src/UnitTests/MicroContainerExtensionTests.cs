using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            string serviceName = null;
            container.AddService<IPerson>(null, person);

            // The container should return the added instance
            var result = container.GetInstance<IPerson>();
            Assert.AreSame(person, result);
        }
    }
}
