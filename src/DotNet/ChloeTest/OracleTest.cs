using Chloe.Extensions;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    class OracleTest
    {
        static string ConnString = "Data Source=localhost/chloe;User ID=system;Password=sa;";
        //static string ConnString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl)));Persist Security Info=True;User ID=system;Password=sa;";

        public static void Test()
        {
            OracleConnection conn = new OracleConnection(ConnString);

            OracleCommand cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT 1 Id,'so' Name,0 F_Bool,'D544BC4C739E4CD3A3D37BF803FCE179' F_Guid,cast(1 as binary_FLOAT),ROWNUM as RN,SYSTIMESTAMP,EXTRACT(year from sysdate),cast(sysdate as TIMESTAMP) as ts,TRUNC(SYSDATE,'DD') TRUNCDATE, cast(:date1 as timestamp) AS SUBTRACTTOTALDAYS,cast(to_timestamp(' 2016-09-07 21:37:31.920000','yyyy-mm-dd hh24:mi:ssxff') as date) as TTT from dual";

            cmd.CommandText = "select  *  from TestEntity";
            cmd.CommandText = "select cast(1 as NUMBER(3,0)),cast(16 as NUMBER(4,0)),cast(1 as NUMBER(9,0)),cast(1 as NUMBER(18,0)),cast(1 as NUMBER(9,2))  from dual";

            cmd.CommandText = "select  (SYSTIMESTAMP-SYSTIMESTAMP) D  from DUAL";
            cmd.CommandText = "select  TO_TIMESTAMP('2016-09-07 21:37:31.920000','yyyy-mm-dd hh24:mi:ssxff') D  from DUAL";

            cmd.CommandText = "select 1,cast(16 as NUMBER(4,0)),cast(1 as NUMBER(9,0)),cast(1 as NUMBER(18,0)),cast(1 as NUMBER(9,2))  from dual";

            conn.Open();
            OracleDataReader reader = cmd.ExecuteReader();
            reader.Read();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                Debug.WriteLine("Name:{0},DataTypeName:{1},FieldType:{2}", reader.GetName(i), reader.GetDataTypeName(i), reader.GetFieldType(i).Name);
            }

            //int ordinal = reader.GetOrdinal("Id");
            //var xx = DataReaderExtensions.GetInt32(reader, ordinal);
            //var xx1 = DataReaderExtensions.GetInt64(reader, ordinal);
            //var f_byte = DataReaderExtensions.GetByte(reader, ordinal);
            //bool b = DataReaderExtensions.GetBoolean(reader, reader.GetOrdinal("F_Bool"));
            //Guid g = DataReaderExtensions.GetGuid(reader, reader.GetOrdinal("F_Guid"));
            //UInt32 val = DataReaderExtensions.GetTValue<UInt32>(reader, ordinal);
            //UInt32? val1 = DataReaderExtensions.GetTValue<UInt32?>(reader, ordinal);
            //var f_dt = DataReaderExtensions.GetDateTime(reader, reader.GetOrdinal("TTT"));

            //var xxx = reader["F_Long"];
            //var s = reader.GetOracleString(0);
            //var t = xxx.GetType();

            //var xxx = reader.GetDateTime(0);

            ConsoleHelper.WriteLineAndReadKey();
        }
    }

}
