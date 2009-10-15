using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Hiro.UnitTests.BugFixes
{
    public class DbConnTest : IDBConnection
    {
        #region IDBConnection Members

        public void Close()
        {

        }

        public SqlConnection CurrentConnection
        {
            get;
            set;
        }

        public string DbConnectionString
        {
            get;
            set;
        }

        #endregion
    }
}
