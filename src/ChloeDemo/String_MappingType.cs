using Chloe;
using Chloe.Infrastructure;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ChloeDemo
{
    class String_MappingType : MappingType<string>, IMappingType
    {
        public String_MappingType() : base(DbType.String)
        {
        }
        public override IDbDataParameter CreateDataParameter(IDbCommand cmd, DbParam param)
        {
            IDbDataParameter parameter = base.CreateDataParameter(cmd, param);

            if (parameter is NpgsqlParameter)
            {
                //For PostgreSQL json
                NpgsqlParameter pgsqlParameter = (NpgsqlParameter)parameter;

                DbType jsonDbType = (DbType)100;
                if (param.DbType == jsonDbType)
                {
                    parameter.DbType = DbType.String;
                    pgsqlParameter.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Json;
                }
                else if (param.DbType != null)
                    parameter.DbType = param.DbType.Value;
            }
            else if (parameter is OracleParameter)
            {
                //For Oracle Clob/NClob

                OracleParameter oracleParameter = (OracleParameter)parameter;

                /* 针对 oracle 长文本做处理 */
                string value = (string)oracleParameter.Value;
                if (value != null && value.Length > 2000)
                {
                    if (param.DbType == DbType.String || param.DbType == DbType.StringFixedLength)
                        oracleParameter.OracleDbType = OracleDbType.NClob;
                    else if (param.DbType == DbType.AnsiString || param.DbType == DbType.AnsiStringFixedLength)
                        oracleParameter.OracleDbType = OracleDbType.Clob;
                }
            }

            return parameter;
        }
    }
}
