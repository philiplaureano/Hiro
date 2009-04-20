 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Hiro.UnitTests.SampleDomain;
using System.Reflection;
using Hiro.Resolvers;
using Hiro.Interfaces;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class ResolutionTests : BaseFixture 
    {
        [Test]
        public void ShouldBeAbleToEnumerateDependenciesFromConstructor()
        {
            var constructor = typeof(Vehicle).GetConstructor(new Type[] { typeof(IPerson) });
            var resolver = new ConstructorDependencyResolver();

            TestResolver(resolver, constructor);
        }

        [Test]
        public void ShouldBeAbleToEnumerateDependenciesFromProperty()
        {
            var property = typeof(Vehicle).GetProperty("Driver");
            var resolver = new PropertyDependencyResolver();

            TestResolver(resolver, property);
        }

        private void TestResolver<TResolver, TTarget>(TResolver resolver, TTarget target)
            where TResolver : IDependencyResolver<TTarget>
            where TTarget : MemberInfo
        {
            var dependencies = resolver.GetDependenciesFrom(target);

            Assert.IsTrue(dependencies.Count() > 0);

            var result = dependencies.First();
            Assert.AreEqual(string.Empty, result.ServiceName);
            Assert.AreEqual(typeof(IPerson), result.ServiceType);
        }
    }
}
