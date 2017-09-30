using Chloe.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    class SqliteTest
    {
        public static void Test()
        {
            SQLiteConnection conn = new SQLiteConnection("Data Source=D:\\MyProject\\Chloe.db;Version=3;Pooling=True;Max Pool Size=100;");

            SQLiteCommand cmd = conn.CreateCommand();

            //cmd.CommandText = "  select * from Users where Id=@Id";
            //cmd.CommandText = "select cast('-9' as INTEGER) from Users";

            cmd.CommandText = "select Cast(strftime('%H','2016-08-06 09:01:24') as INTEGER)";

            //cmd.Parameters.Add(new SQLiteParameter("@Id", 1));

            conn.Open();
            
            SQLiteDataReader reader = cmd.ExecuteReader();

            //reader.NextResult();
            bool r = reader.Read();
            //r = DataReaderExtensions.GetBoolean(reader, 0);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Console.WriteLine("Name:{0},DataTypeName:{1},FieldType:{2}", reader.GetName(i), reader.GetDataTypeName(i), reader.GetFieldType(i).Name);
            }

            object ret = null;

            int ordinal = reader.GetOrdinal("Id");
            ret = reader.GetValue(0);
            Console.WriteLine(ret.GetType().Name);
            //ret = DataReaderExtensions.GetDateTime(reader, 0);
            var xx = DataReaderExtension.GetInt32(reader, 0);
            //var xx = DataReaderExtensions.GetInt64(reader, 0);
            //var xx = DataReaderExtensions.GetDecimal(reader, 0);
            //var xx = DataReaderExtensions.GetGuid(reader, 0);
            //UInt32 val = DataReaderExtensions.GetTValue<UInt32>(reader, ordinal);
            //UInt32? val1 = DataReaderExtensions.GetTValue<UInt32?>(reader, ordinal);

            ConsoleHelper.WriteLineAndReadKey();
        }

    }
}
