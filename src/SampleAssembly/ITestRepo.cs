using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiro.UnitTests.BugFixes
{
    public interface ITestRepo
    {
        IDBConnection mDbConnection { get; set; }
    }
}
