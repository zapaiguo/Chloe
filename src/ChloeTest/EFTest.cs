//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Linq;
//using Microsoft.EntityFrameworkCore;

//namespace ChloeTest
//{
//    class EFTest
//    {
//        public static void Run()
//        {
//            object result = null;

//            EFContext context = new EFContext();

//            //var result = context.Users.ToList();
//            int? id = null;

//            FormattableString s = $"select * from Users where id>{id}";

//            result = context.Users.FromSql($"select * from Users where id>{id}").ToList();

//            ConsoleHelper.WriteLineAndReadKey();
//        }
//    }
//}
