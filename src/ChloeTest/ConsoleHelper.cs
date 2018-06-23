using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    class ConsoleHelper
    {
        public static void WriteLineAndReadKey(object val)
        {
            Console.WriteLine(val);
            Console.ReadKey();
        }

        public static void WriteLineAndReadKey(string val = "...")
        {
            Console.WriteLine(val);
            Console.ReadKey();
        }

        public static void WriteReaderInfo(IDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Console.WriteLine("Name:{0},DataTypeName:{1},FieldType:{2}", reader.GetName(i), reader.GetDataTypeName(i), reader.GetFieldType(i).Name);
            }

            //int ordinal = reader.GetOrdinal("UInt");
            //var xx = DataReaderExtensions.GetInt32(reader, ordinal);
            //UInt32 val = DataReaderExtensions.GetTValue<UInt32>(reader, ordinal);
            //UInt32? val1 = DataReaderExtensions.GetTValue<UInt32?>(reader, ordinal);

            ConsoleHelper.WriteLineAndReadKey();
        }
    }
}
