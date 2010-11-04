using System;
using Hiro.Containers;
using Hiro.Functors.UnitTests.SampleModel;
using NUnit.Framework;

namespace Hiro.Functors.UnitTests
{
    [TestFixture]
    public class FunctorTests
    {
        [Test]
        public void ShouldBeAbleToAddNamedTypedFunctorToDependencyMap()
        {
            var expectedInstance = new Foo();
            Func<IMicroContainer, IFoo> functor = c => expectedInstance;

            var map = new DependencyMap();
            map.AddService("myFoo", functor);

            var container = map.CreateContainer();
            var foo = container.GetInstance<IFoo>("myFoo");

            Assert.AreSame(expectedInstance, foo);
        }


        [Test]
        public void ShouldBeAbleToAddTypedFunctorToDependencyMap()
        {
            var expectedInstance = new Foo();
            Func<IMicroContainer, IFoo> functor = c => expectedInstance;

            var map = new DependencyMap();
            map.AddService(functor);

            var container = map.CreateContainer();
            var foo = container.GetInstance<IFoo>();

            Assert.AreSame(expectedInstance, foo);
        }

        [Test]
        public void ShouldBeAbleToAddNamedUntypedFunctorToDependencyMap()
        {
            var expectedInstance = new Foo();
            Func<IMicroContainer, object> functor = c => expectedInstance;

            var serviceType = typeof(IFoo);
            var map = new DependencyMap();
            map.AddService("myFoo", serviceType, functor);

            var container = map.CreateContainer();
            var foo = container.GetInstance<IFoo>("myFoo");
            Assert.IsNotNull(foo);
            Assert.AreSame(expectedInstance, foo);
        }

        [Test]
        public void ShouldBeAbleToAddUntypedFunctorToDependencyMap()
        {
            var expectedInstance = new Foo();
            Func<IMicroContainer, object> functor = c => expectedInstance;

            var serviceType = typeof(IFoo);
            var map = new DependencyMap();
            map.AddService(serviceType, functor);

            var container = map.CreateContainer();
            var foo = container.GetInstance<IFoo>();
            Assert.IsNotNull(foo);
            Assert.AreSame(expectedInstance, foo);
        }
    }
}

