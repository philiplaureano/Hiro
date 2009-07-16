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
using Moq;

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
            foreach (var service in list)
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

        [Test]
        public void ShouldLoadServicesUsingTheGivenServiceLoaderAndAssemblyLoaderAndServiceResolverInstances()
        {
            var serviceLoader = new Mock<IServiceLoader>();
            var resolver = new Mock<IDefaultServiceResolver>();
            var assemblyLoader = new Mock<IAssemblyLoader>();

            var assembly = typeof(IPerson).Assembly;
            var assemblies = new Assembly[] { assembly };
            var serviceType = typeof(IVehicle);
            var serviceList = new List<IServiceInfo>();
            IEnumerable<IServiceInfo> services = serviceList;

            var defaultService = new ServiceInfo(typeof(IVehicle), typeof(Vehicle), "Vehicle");

            serviceList.Add(new ServiceInfo(typeof(IVehicle), typeof(Truck), "Truck"));
            serviceList.Add(defaultService);

            resolver.Expect(r => r.GetDefaultService(It.IsAny<Type>(), It.IsAny<IEnumerable<IServiceInfo>>())).Returns(defaultService);
            serviceLoader.Expect(s => s.Load(assembly)).Returns(services);

            var loader = new DependencyMapLoader(serviceLoader.Object, resolver.Object);
            DependencyMap map = loader.LoadFrom(assemblies);

            resolver.VerifyAll();
            serviceLoader.VerifyAll();

            // Make sure the services are loaded into the dependency map
            Assert.IsTrue(map.Contains(new Dependency(typeof(IVehicle), "Vehicle")));
            Assert.IsTrue(map.Contains(new Dependency(typeof(IVehicle), "Truck")));
            Assert.IsTrue(map.Contains(new Dependency(typeof(IVehicle))));
        }

        [Test]
        public void ShouldLoadAndCreateContainerFromSampleAssembly()
        {
            var loader = new DependencyMapLoader();
            var assemblies = new Assembly[] { typeof(IPerson).Assembly };

            var map = loader.LoadFrom(assemblies);
            var container = map.CreateContainer();

            Assert.IsNotNull(container);

            var vehicle = container.GetInstance<IVehicle>();
            Assert.IsNotNull(vehicle);
        }

        [Test]
        public void ShouldLoadAndCreateContainerFromSingleAssembly()
        {
            var loader = new DependencyMapLoader();

            var map = loader.LoadFrom(typeof(IPerson).Assembly);
            var container = map.CreateContainer();

            Assert.IsNotNull(container);

            var vehicle = container.GetInstance<IVehicle>();
            Assert.IsNotNull(vehicle);
        }

        [Test]
        public void ShouldLoadAndCreateContainerFromDirectory()
        {
            var loader = new DependencyMapLoader();

            var map = loader.LoadFrom(AppDomain.CurrentDomain.BaseDirectory, "*.UnitTests.dll");
            var container = map.CreateContainer();

            Assert.IsNotNull(container);

            var vehicle = container.GetInstance<IVehicle>();
            Assert.IsNotNull(vehicle);
        }
    }
}
