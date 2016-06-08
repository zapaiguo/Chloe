using Chloe.Core.Visitors;
using Chloe.Infrastructure;
using Chloe.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Chloe.SqlServer
{
    class DbContextServiceProvider : IDbContextServiceProvider
    {
        string _connString;

        public DbContextServiceProvider(string connString)
        {
            this._connString = connString;
        }
        public IDbConnection CreateConnection()
        {
            SqlConnection conn = new SqlConnection(this._connString);
            return conn;
        }
        public IDbExpressionTranslator CreateDbExpressionTranslator()
        {
            return DbExpressionTranslator.Instance;
        }
    }
}
