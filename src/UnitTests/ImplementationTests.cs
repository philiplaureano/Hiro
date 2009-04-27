using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Implementations;
using Hiro.Interfaces;
using Hiro.Resolvers;
using Hiro.UnitTests.SampleDomain;
using Moq;
using NUnit.Framework;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class ImplementationTests : BaseFixture
    {
        [Test]
        public void ShouldSelectConstructorWithMostResolvableParameters()
        {
            var targetType = typeof(Vehicle);
            var constructorImplementations = (from c in typeof(Vehicle).GetConstructors()
                                              select new ConstructorImplementation(c) as IImplementation<ConstructorInfo>).AsEnumerable();

            var bestParameterCount = 0;
            IImplementation<ConstructorInfo> expectedImplementation = null;
            foreach (var implementation in constructorImplementations)
            {
                var target = implementation.Target;
                var parameterCount = target.GetParameters().Count();

                if (expectedImplementation != null && parameterCount <= bestParameterCount)
                    continue;

                expectedImplementation = implementation;
                bestParameterCount = parameterCount;
            }

            var map = new Mock<IDependencyContainer>();
            map.Expect(m => m.Contains(It.IsAny<Dependency>())).Returns(true);

            var constructorResolver = new ConstructorResolver(constructorImplementations);
            IImplementation<ConstructorInfo> result = constructorResolver.ResolveFrom(map.Object);

            Assert.AreEqual(expectedImplementation.Target, result.Target);

            map.VerifyAll();
        }
    }
}
