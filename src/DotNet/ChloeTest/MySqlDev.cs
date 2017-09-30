using Chloe.Extensions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    class MySqlDev
    {
        static string ConnString = "Database='Chloe';Data Source=localhost;User ID=root;Password=sasa;CharSet=utf8;";

        public static void Test()
        {
            MySqlConnection conn = new MySqlConnection(ConnString);

            MySqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = "select * from users";

            conn.Open();

            MySqlDataReader reader = cmd.ExecuteReader();

            reader.Read();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                Console.WriteLine("Name:{0},DataTypeName:{1},FieldType:{2}", reader.GetName(i), reader.GetDataTypeName(i), reader.GetFieldType(i).Name);
            }

            int ordinal = reader.GetOrdinal("UInt");
            var xx = DataReaderExtension.GetInt32(reader, ordinal);
            UInt32 val = DataReaderExtension.GetTValue<UInt32>(reader, ordinal);
            UInt32? val1 = DataReaderExtension.GetTValue<UInt32?>(reader, ordinal);

            ConsoleHelper.WriteLineAndReadKey();
        }
    }
}
