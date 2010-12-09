using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiro.UnitTests.BugFixes.LightSpeed
{
    public class SimpleUnitOfWorkScope<T> : UnitOfWorkScopeBase<T>
    {
        public SimpleUnitOfWorkScope(LightSpeedContext<T> userUnitOfWork)
        {            
        }
    }
}
