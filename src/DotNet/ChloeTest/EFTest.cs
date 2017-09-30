using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChloeTest;
using System.Diagnostics;

namespace ChloeTest
{
    class EFTest
    {
        public static void Test()
        {
            EFContext context = new EFContext();

            double d = 123456.123456;

            //var xxxx = context.TestEntity.Select(a => new { D = DbFunctions.DiffMicroseconds(DateTime.Now, DateTime.Now) }).ToString();
            //int age
           

            var s = context.TestEntity.Select(a =>
                new
                {
                    F_Int16 = (decimal)a.F_Int16,
                    F_Int32 = (decimal)a.F_Int32,
                    F_Int64 = (decimal)a.F_Int64,
                    F_Byte = (decimal)a.F_Byte,
                }

                ).ToString();
            //var xx = context.TestEntity.Select(a => (decimal)a.F_Float).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }
    }
}
