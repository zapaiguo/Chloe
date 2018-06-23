using Chloe.Infrastructure.Interception;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    class DbCommandInterceptor : IDbCommandInterceptor
    {
        /* 执行 DbCommand.ExecuteReader() 时调用 */
        public void ReaderExecuting(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            interceptionContext.DataBag.Add("startTime", DateTime.Now);
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void ReaderExecuted(IDbCommand command, DbCommandInterceptionContext<IDataReader> interceptionContext)
        {
            DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
            Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result.FieldCount);
        }

        public void NonQueryExecuting(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            interceptionContext.DataBag.Add("startTime", DateTime.Now);
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void NonQueryExecuted(IDbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
            Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result);
        }

        public void ScalarExecuting(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            interceptionContext.DataBag.Add("startTime", DateTime.Now);
            Debug.WriteLine(AppendDbCommandInfo(command));
            Console.WriteLine(command.CommandText);
            AmendParameter(command);
        }
        public void ScalarExecuted(IDbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            DateTime startTime = (DateTime)(interceptionContext.DataBag["startTime"]);
            Console.WriteLine(DateTime.Now.Subtract(startTime).TotalMilliseconds);
            if (interceptionContext.Exception == null)
                Console.WriteLine(interceptionContext.Result);
        }

        static void AmendParameter(IDbCommand command)
        {
            foreach (var parameter in command.Parameters)
            {
                if (parameter is OracleParameter)
                {
                    OracleParameter oracleParameter = (OracleParameter)parameter;
                    if (oracleParameter.Value is string)
                    {
                        /* 针对 oracle 长文本做处理 */
                        string value = (string)oracleParameter.Value;
                        if (value != null && value.Length > 2000)
                        {
                            if (oracleParameter.DbType == DbType.String || oracleParameter.DbType == DbType.StringFixedLength)
                                oracleParameter.OracleDbType = OracleDbType.NClob;
                            else if (oracleParameter.DbType == DbType.AnsiString || oracleParameter.DbType == DbType.AnsiStringFixedLength)
                                oracleParameter.OracleDbType = OracleDbType.Clob;
                        }
                    }
                }
                else if (parameter is NpgsqlParameter)
                {
                    //NpgsqlParameter pgsqlParameter = (NpgsqlParameter)parameter;
                    //DbType jsonDbType = (DbType)100;
                    //if (pgsqlParameter.DbType == jsonDbType)
                    //{
                    //    pgsqlParameter.DbType = DbType.String;
                    //    pgsqlParameter.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Json;
                    //}
                }

            }
        }


        static string AppendDbCommandInfo(IDbCommand command)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IDbDataParameter param in command.Parameters)
            {
                if (param == null)
                    continue;

                object value = null;
                if (param.Value == null || param.Value == DBNull.Value)
                {
                    value = "NULL";
                }
                else
                {
                    value = param.Value;

                    if (param.DbType == DbType.String || param.DbType == DbType.DateTime)
                        value = "'" + value + "'";
                }

                sb.AppendFormat("{3} {0} {1} = {2};", Enum.GetName(typeof(DbType), param.DbType), param.ParameterName, value, Enum.GetName(typeof(ParameterDirection), param.Direction));
                sb.AppendLine();
            }

            sb.AppendLine(command.CommandText);

            return sb.ToString();
        }
    }
}
