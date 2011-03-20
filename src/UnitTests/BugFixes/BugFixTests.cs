using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;
using Hiro.UnitTests.BugFixes.LightSpeed;
using Hiro.UnitTests.SampleDomain;
using Mono.Cecil;
using NUnit.Framework;
using Hiro.Loaders;
using PaulBenchmark;

namespace Hiro.UnitTests.BugFixes
{
    [TestFixture]
    public class BugFixTests
    {
        [Test]
        public void ShouldLoadAllImplementationsAndInterfaces()
        {
            var assembly = typeof(IDBConnection).Assembly;

            var loader = new DependencyMapLoader();
            var map = loader.LoadFrom(assembly);
            var container = map.CreateContainer();

            var testRepo = container.GetInstance<ITestRepo>();
            Assert.IsNotNull(testRepo);
        }

        [Test]
        public void ShouldBeAbleToInjectGenericConstructor()
        {
            var map = new DependencyMap();
            map.AddService<UnitOfWorkScopeBase<UserUnitOfWork>,
            SimpleUnitOfWorkScope<UserUnitOfWork>>();

            map.AddSingletonService<LightSpeedContext<UserUnitOfWork>,
            LightSpeedContext<UserUnitOfWork>>();

            var container = map.CreateContainer();
            var service = container.GetInstance<UnitOfWorkScopeBase<UserUnitOfWork>>();

            Assert.IsNotNull(service);
        }

        [Test]
        public void ShouldBeAbleToCreateSingletonsThatDependOnOtherSingletons()
        {
            var map = new DependencyMap();
            map.AddSingletonService(typeof(IVehicle), typeof(Vehicle));
            map.AddSingletonService(typeof(IPerson), typeof(Person));

            var compiler = map.ContainerCompiler;
            var outputAssembly = compiler.Compile("MicroContainer", "Hiro.Containers", "Hiro.CompiledContainers", map);
            outputAssembly.Write("singletonOutputAssembly.dll");

            var container = map.CreateContainer();
            var vehicle = container.GetInstance<IVehicle>();
            Assert.IsNotNull(vehicle);

            var person = container.GetInstance<IPerson>();
            Assert.IsNotNull(person);
            for (var i = 0; i < 1000; i++)
            {
                var currentInstance = container.GetInstance<IVehicle>();
                Assert.AreSame(vehicle, currentInstance);

                var driver = currentInstance.Driver;
                Assert.AreSame(driver, person);
            }
        }

        [Test]
        public void ShouldBeAbleToResolvePlayerInstanceWithoutEncounteringACLRLimitationError()
        {
            var map = new DependencyMap();
            map.AddSingletonService<Game, Game>();
            map.AddService<Player, Player>();
            map.AddService<Gun, Gun>();
            map.AddService<Bullet, Bullet>();

            Func<IMicroContainer, Bullet> createBullet = c => c.GetInstance<Bullet>();
            Func<IMicroContainer, Func<Bullet>> createBulletFunctor = c => () => createBullet(c);
            map.AddService(createBulletFunctor);

            var container = map.CreateContainer();
            Assert.IsNotNull(container);

            var player = container.GetInstance<Player>();
            Assert.IsNotNull(player);

            player.Shoot();
        }
    }
}
