using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Chloe.MySql
{
    public class MySqlContext : DbContext
    {
        private string _connectionString;

        public MySqlContext(string connectionString)
            : base(SqlProvider.Instance)
        {
            _connectionString = connectionString;
        }

        public override IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }

    }
}
