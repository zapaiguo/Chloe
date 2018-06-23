using Chloe.Infrastructure;
using Chloe.PostgreSQL;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ChloeTest
{
    class PostgreSQLTest
    {
        static string ConnString = "User ID=postgres;Password=sa;Host=localhost;Port=5432;Database=chloe;Pooling=true;";
        static PostgreSQLContext context = new PostgreSQLContext(new PostgreSQLConnectionFactory(ConnString));
        public static void Test()
        {

            CTest();
            //InsertRangeTest();

            //QueryTest();
            //SqlQueryTest();
            //PredicateTest();
            // MappingTest();
            //MethodTest();
            //JoinQueryTest();
            //InsertTest();
            //UpdateTest();
            //DeleteTest();
            //AggregateFunctionTest();
            //GroupQueryTest();
            // TrackingTest();


            //BaseQuery();
            //JoinQuery();
            //AFQuery();
            //GQTest();
            //Insert();

            //UpdateTest1();
            //DeleteTest1();
            //MethodTest1();

            ConsoleHelper.WriteLineAndReadKey();
        }

        public static void CTest()
        {
            object result = null;
            List<int> ids = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            var q = context.Query<User>();

            DateTime dt = DateTime.Now;
            string name = "hu";
            string name1 = "lu";
            string nameNull = null;
            string ageString = "18";
            Int16 shortId = 2;
            int id = 2;
            long longId = 2;
            Gender gender = Gender.Man;


            User user = new User();
            user.Name = "qinshuxin";
            user.Age = 188;
            user.CityId = 1;
            user.OpTime = DateTime.Now;
            //user.TimeSpan = TimeSpan.FromDays(2);
            //context.Insert(user);

            //result = q.ToList();
            //result = q.Select(a => new
            //{
            //    t = (TimeSpan?)(dt.Subtract(a.OpTime.Value)),
            //    t2 = (TimeSpan?)(dt.Subtract(dt.AddMinutes(-60)))
            //}).ToList();


            var reader = context.Session.ExecuteReader("SELECT DATE_PART('YEAR',NOW()) AS \"year\" FROM \"users\" AS \"users\"");
            reader.Read();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Console.WriteLine($"{reader.GetName(i)}, {reader.GetFieldType(i).Name}, {reader.GetDataTypeName(i)}");
                Console.WriteLine(reader.GetInt32(i));
            }
            reader.Close();


            TestEntity t = new TestEntity();
            t.F_Byte = 1;
            t.F_Int16 = 16;
            t.F_Int32 = 32;
            t.F_Int64 = 64;
            t.F_Double = 1.2;
            t.F_Float = 1.1f;
            t.F_Decimal = 1.112m;
            t.F_Bool = true;
            t.F_DateTime = DateTime.Now;
            t.F_String = "soooo";
            t.F_Json = "{\"id\": 10,\"age\":18}";

            context.Insert(t);

            var list = context.Query<TestEntity>().OrderByDesc(a => a.Id).ToList();

            t = list.First();
            t.F_Bool = false;
            //context.Delete(t);

            ConsoleHelper.WriteLineAndReadKey();
        }

    }

}
