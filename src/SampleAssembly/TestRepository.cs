using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiro.UnitTests.BugFixes
{
    public class TestRepository : ITestRepo
    {
        public TestRepository(IDBConnection dbConnection)
        {
            mDbConnection = dbConnection;
        }

        public IDBConnection mDbConnection
        {
            get;
            set;
        }
    }
}
