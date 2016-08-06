using Chloe.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo
{
    class SQLiteDemo
    {
        public static void Test()
        {
            SQLiteContext context = new SQLiteContext(new SQLiteConnectionFactory("Data Source=..\\..\\Chloe.db;Version=3;Pooling=True;Max Pool Size=100;"));

            var q = context.Query<User>();

            var users = q.Take(10).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }
    }
}
