using Chloe.Data;
using Chloe.Infrastructure;
using Chloe.Oracle;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo
{
    public class OracleConnectionFactory : IDbConnectionFactory
    {
        string _connString = null;
        public OracleConnectionFactory(string connString)
        {
            this._connString = connString;
        }
        public IDbConnection CreateConnection()
        {
            OracleConnection oracleConnection = new OracleConnection(this._connString);
            OracleConnectionDecorator conn = new OracleConnectionDecorator(oracleConnection);
            return conn;
        }
    }

    class OracleConnectionDecorator : DbConnectionDecorator, IDbConnection, IDisposable
    {
        OracleConnection _oracleConnection;
        public OracleConnectionDecorator(OracleConnection oracleConnection) : base(oracleConnection)
        {
            this._oracleConnection = oracleConnection;
        }

        public override IDbCommand CreateCommand()
        {
            var cmd = this._oracleConnection.CreateCommand();
            cmd.BindByName = true;
            return cmd;
        }
    }
}
