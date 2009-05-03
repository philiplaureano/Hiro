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
    }
}
