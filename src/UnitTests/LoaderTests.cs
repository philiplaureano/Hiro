using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Containers;
using Hiro.Interfaces;
using Hiro.Loaders;
using Hiro.UnitTests.SampleDomain;
using Moq;
using NGenerics.DataStructures.General;
using NUnit.Framework;
using SampleAssembly;

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

            var items = new List<IServiceInfo>(list);
            Assert.IsTrue(items.Count > 0);
        }

        [Test]
        public void ShouldLoadTypeImplementationsHavingTheirTypeNameAsTheServiceName()
        {
            var loader = new ServiceLoader();
            IEnumerable<IServiceInfo> list = loader.Load(hostAssembly);

            var expectedService = new ServiceInfo(typeof(IPerson), typeof(Person), "Person");
            var serviceList = new List<IServiceInfo>(list);
            Assert.IsTrue(serviceList.Contains(expectedService));
        }

        [Test]
        public void ShouldBeAbleToAutomaticallyInjectBaseGenericInterfaceTypes()
        {
            var loader = new DependencyMapLoader();
            var map = loader.LoadFromBaseDirectory("Sample*.dll");
            map.AddService<IFoo<int>, SampleGenericImplementation>();

            var dependencies = map.Dependencies;

            var container = map.CreateContainer();
            Assert.IsTrue(container.Contains(typeof(IBaz<int>), "SampleGenericImplementation"));
            Assert.IsTrue(container.Contains(typeof(IFoo<int>), "SampleGenericImplementation"));
        }

        [Test]
        public void ShouldNotHaveDefaultServicesWhenLoadingTypesFromAssembly()
        {
            var loader = new ServiceLoader();
            IEnumerable<IServiceInfo> list = loader.Load(hostAssembly);

            var items = new List<IServiceInfo>(list);
            Assert.IsTrue(items.Count > 0);
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
            var typeLoader = new Mock<ITypeLoader>();

            var assembly = typeof(IPerson).Assembly;
            var assemblies = new Assembly[] { assembly };
            var serviceList = new List<IServiceInfo>();
            IEnumerable<IServiceInfo> services = serviceList;

            var defaultService = new ServiceInfo(typeof(IVehicle), typeof(Vehicle), "Vehicle");

            serviceList.Add(new ServiceInfo(typeof(IVehicle), typeof(Truck), "Truck"));
            serviceList.Add(defaultService);

            typeLoader.Expect(l => l.LoadTypes(It.IsAny<Assembly>())).Returns(new System.Type[0]);
            resolver.Expect(r => r.GetDefaultService(It.IsAny<System.Type>(), It.IsAny<IEnumerable<IServiceInfo>>())).Returns(defaultService);
            serviceLoader.Expect(s => s.Load(assembly)).Returns(services);

            var loader = new DependencyMapLoader(typeLoader.Object, serviceLoader.Object, resolver.Object);
            DependencyMap map = loader.LoadFrom(assemblies);

            typeLoader.VerifyAll();
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

        [Test]
        public void ShouldLoadAndCreateContainerFromBaseDirectory()
        {
            var loader = new DependencyMapLoader();

            var map = loader.LoadFromBaseDirectory("*.UnitTests.dll");
            var container = map.CreateContainer();

            Assert.IsNotNull(container);

            var vehicle = container.GetInstance<IVehicle>();
            Assert.IsNotNull(vehicle);
        }

        [Test]
        public void ShouldBeAbleToFilterLoadedServicesUsingASinglePredicate()
        {
            Predicate<IServiceInfo> serviceFilter = service => service.ServiceType != typeof(IVehicle);

            var loader = new DependencyMapLoader();
            loader.ServiceFilter = serviceFilter;

            var map = loader.LoadFrom(AppDomain.CurrentDomain.BaseDirectory, "*.UnitTests.dll");
            var container = map.CreateContainer();

            Assert.IsNotNull(container);

            // There should be no IVehicle container instances
            // in this container since the IVehicle service has been filtered
            // out of the container
            var vehicle = container.GetInstance<IVehicle>();
            Assert.IsNull(vehicle);
        }

        [Test]
        public void ShouldBeAbleToLoadEnumerableServicesByDefault()
        {
            var loader = new DependencyMapLoader();
            loader.ServiceFilter = info => !string.IsNullOrEmpty(info.ServiceName) && info.ServiceName.StartsWith("Baz") 
                && info.ServiceType == typeof(IBaz<int>) || info.ServiceType == typeof(IFizz);
            var map = loader.LoadFrom(typeof(IFoo<>).Assembly);
            var container = map.CreateContainer();

            Assert.IsNotNull(container);

            var fizz = container.GetInstance<IFizz>();
            Assert.IsNotNull(fizz);

            Assert.AreEqual(3, fizz.Services.Count());

            var services = fizz.Services.ToArray();
            Assert.IsInstanceOfType(typeof(Baz1), services[0]);
            Assert.IsInstanceOfType(typeof(Baz2), services[1]);
            Assert.IsInstanceOfType(typeof(Baz3), services[2]);
        }
    }
}
