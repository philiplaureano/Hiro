using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Containers;
using Hiro.Implementations;
using Hiro.Interfaces;
using Hiro.Resolvers;
using Hiro.UnitTests.SampleDomain;
using Mono.Cecil;
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
                                              select new ConstructorCall(c) as IStaticImplementation<ConstructorInfo, MethodDefinition>).AsEnumerable();

            IStaticImplementation<ConstructorInfo, MethodDefinition> expectedStaticImplementation = GetExpectedConstructorImplementation(constructorImplementations);

            var map = new Mock<IDependencyContainer<MethodDefinition>>();
            map.Expect(m => m.Contains(It.IsAny<Dependency>())).Returns(true);
            map.Expect(m => m.Dependencies).Returns(new IDependency[] {});

            var constructorResolver = new ConstructorResolver();
            IStaticImplementation<ConstructorInfo, MethodDefinition> result = constructorResolver.ResolveFrom(targetType, map.Object);

            Assert.AreEqual(expectedStaticImplementation.Target, result.Target);

            map.VerifyAll();
        }

        [Test]
        public void ShouldSelectConstructorWithMostResolvableParametersFromTypeImplementation()
        {
            var map = new Mock<IDependencyContainer<MethodDefinition>>();
            map.Expect(m => m.Contains(It.IsAny<Dependency>())).Returns(true);
            map.Expect(m => m.Dependencies).Returns(new IDependency[] { });

            var expectedConstructor = typeof(Vehicle).GetConstructor(new Type[] { typeof(IPerson) });
            IStaticImplementation<ConstructorInfo, MethodDefinition> staticImplementation = new TransientType<MethodDefinition>(typeof(Vehicle), map.Object, new ConstructorResolver());

            Assert.AreSame(staticImplementation.Target, expectedConstructor);
        }

        private static IStaticImplementation<ConstructorInfo, MethodDefinition> GetExpectedConstructorImplementation(IEnumerable<IStaticImplementation<ConstructorInfo, MethodDefinition>> constructorImplementations)
        {
            var bestParameterCount = 0;
            IStaticImplementation<ConstructorInfo, MethodDefinition> expectedStaticImplementation = null;
            foreach (var implementation in constructorImplementations)
            {
                var target = implementation.Target;
                var parameterCount = target.GetParameters().Count();

                if (expectedStaticImplementation != null && parameterCount <= bestParameterCount)
                    continue;

                expectedStaticImplementation = implementation;
                bestParameterCount = parameterCount;
            }
            return expectedStaticImplementation;
        }
    }
}
