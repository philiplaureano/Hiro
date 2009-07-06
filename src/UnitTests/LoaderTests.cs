using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Loaders;
using Hiro.UnitTests.SampleDomain;
using NUnit.Framework;
using NGenerics.DataStructures.General;
using Hiro.Interfaces;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class LoaderTests : BaseFixture
    {
        private Assembly hostAssembly = typeof(IPerson).Assembly;

        [Test]
        public void ShouldLoadAssemblyIntoMemory()
        {
            string filename = hostAssembly.Location;

            var loader = new AssemblyLoader();
            Assembly assembly = loader.Load(filename);
            Assert.IsNotNull(assembly);
        }

        [Test]
        public void ShouldLoadImplementationMapIntoMemory()
        {
            var loader = new ServiceLoader();
            IEnumerable<IServiceInfo> list = loader.Load(hostAssembly);

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count() > 0);
        }

        [Test]
        public void ShouldLoadTypeImplementationsHavingTheirTypeNameAsTheServiceName()
        {
            var loader = new ServiceLoader();
            IEnumerable<IServiceInfo> list = loader.Load(hostAssembly);

            var expectedService = new ServiceInfo(typeof(IPerson), typeof(Person), "Person");
            var serviceList = new HashSet<IServiceInfo>(list);
            Assert.IsTrue(serviceList.Contains(expectedService));
        }

        [Test]
        public void ShouldNotHaveDefaultServicesWhenLoadingTypesFromAssembly()
        {
            var loader = new ServiceLoader();
            IEnumerable<IServiceInfo> list = loader.Load(hostAssembly);

            Assert.IsTrue(list.Count() > 0);
            foreach(var service in list)
            {
                var serviceName = service.ServiceName;
                Assert.IsFalse(string.IsNullOrEmpty(serviceName));
            }
        }

        [Test]
        public void ShouldAssignDefaultServiceWhenMultipleImplementationsOfTheSameInterfaceExist()
        {
            var loader = new ServiceLoader();
            IEnumerable<IServiceInfo> list = loader.Load(hostAssembly);

            var serviceResolver = new DefaultServiceResolver();
            IServiceInfo serviceInfo = serviceResolver.GetDefaultService(typeof(IVehicle), list);

            Assert.AreEqual("Vehicle", serviceInfo.ServiceName);
            Assert.AreEqual(typeof(IVehicle), serviceInfo.ServiceType);
            Assert.AreEqual(typeof(Vehicle), serviceInfo.ImplementingType);
        }
    }
}
