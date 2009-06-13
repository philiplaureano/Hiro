using System;
using NUnit.Framework;

namespace Hiro.UnitTests
{
    public abstract class BaseFixture
    {

        [SetUp]
        public void Init()
        {
            OnInit();
        }

        [TearDown]
        public void Term()
        {
            OnTerm();
        }

        protected virtual void OnInit() { }
        protected virtual void OnTerm() { }
    }
}
