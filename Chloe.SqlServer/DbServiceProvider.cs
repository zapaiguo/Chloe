using Chloe.Core.Visitors;
using Chloe.Infrastructure;
using Chloe.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.SqlServer
{
    class DbServiceProvider : IDbServiceProvider
    {
        string _connString;
        public DbServiceProvider(string connString)
        {
            this._connString = connString;
        }
        public IDbConnection CreateConnection()
        {
            SqlConnection conn = new SqlConnection(this._connString);
            return conn;
        }
        public AbstractDbExpressionVisitor CreateDbExpressionVisitor()
        {
            AbstractDbExpressionVisitor visitor = SqlExpressionVisitor.CreateInstance();
            return visitor;
        }
    }
}
