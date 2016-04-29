using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace Chloe.Oracle
{
    public class OracleContext : DbContext
    {
        private string _connectionString;

        public OracleContext(string connectionString)
            : base(SqlProvider.Instance)
        {
            _connectionString = connectionString;
        }

        public override IDbConnection CreateConnection()
        {
            return new ChloeOracleConnection(new OracleConnection(_connectionString));
        }

    }
}
