using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Hiro.UnitTests.BugFixes
{
    public interface IDBConnection
    {
        void Close();
        SqlConnection CurrentConnection { get; set; }
        string DbConnectionString { get; set; }
    }
}
