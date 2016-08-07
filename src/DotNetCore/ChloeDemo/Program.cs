using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChloeDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var xx = Guid.Parse("D544BC4C739E4CD3A3D37BF803FCE179");

            SQLiteDemo.Test();
            //MsSqlTest.Test();
            //MySqlDemo.Test();
        }
    }
}
