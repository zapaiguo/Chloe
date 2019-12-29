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
            /*
             * 修改参数绑定方式有两个途径：
             * 1. 使用如下 OracleConnectionDecorator 的方式
             * 2. 使用拦截器修改，在 IDbCommandInterceptor.ReaderExecuting()，IDbCommandInterceptor.NonQueryExecuting()，IDbCommandInterceptor.ScalarExecuting() 方法里对 DbCommand 做处理，参考 ChloeDemo.DbCommandInterceptor 类
             */

            OracleConnection oracleConnection = new OracleConnection(this._connString);
            OracleConnectionDecorator conn = new OracleConnectionDecorator(oracleConnection);
            return oracleConnection;
        }
    }

    /// <summary>
    /// 该装饰器主要修改参数绑定方式。
    /// </summary>
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
