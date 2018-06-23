using Chloe;
using Chloe.Core;
using Chloe.Core.Visitors;
using Chloe.Infrastructure.Interception;
using Chloe.SqlServer;
using Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
//CompilerGeneratedAttribute

namespace ChloeTest
{
    //[TestClass]
    public static class FeatureTest
    {
        static readonly int ID = 9999;
        static readonly User _User = new User();
        static MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

        public static void Test()
        {
            FeatureTest.CTest();
            //BulkCopyTest();
            //InsertRangeTest();

            //FeatureTest.QueryTest();
            //FeatureTest.SqlQueryTest();
            //FeatureTest.PredicateTest();
            //FeatureTest.MappingTest();
            //FeatureTest.MethodTest();
            FeatureTest.JoinQueryTest();
            //FeatureTest.InsertTest();
            //FeatureTest.UpdateTest();
            //FeatureTest.DeleteTest();
            //FeatureTest.AggregateFunctionTest();
            //FeatureTest.GroupQueryTest();
            //FeatureTest.TrackingTest();

            //FeatureTest.JTest();
            //FeatureTest.AFTest();
            //FeatureTest.GQTest();
            //FeatureTest.GroupJion();
            //FeatureTest.QTest();
            //FeatureTest.MTest();
            //FeatureTest.InsertTest1();
            //FeatureTest.UpdateTest1();
            //FeatureTest.DeleteTest1();

            ConsoleHelper.WriteLineAndReadKey();
        }
        public static bool In<T>(this T obj, IEnumerable<T> source)
        {
            return source.Contains(obj);
        }
        //public static bool In<T>(this T obj, List<T> list)
        //{
        //    return list.Contains(obj);
        //}

        public static void CTest()
        {
            object result = null;
            List<int> ids = new List<int>() { 1, 2, 3 };
            List<string> names = new List<string>() { "lu", "shuxin" };
            var qt = context.Query<TestEntity>();
            var q = context.Query<User>();

            var d = DateTime.Now;

            string s = q.ToString();

            string name = "lu";
            string name1 = "lu";
            string nameNull = null;
            string ageString = "18";
            int id = 2;
            long longId = 2;

            result = q.Where(a => a.OpTime != null).Select(a => a.OpTime.Value.AddDays(id)).ToList();


            ConsoleHelper.WriteLineAndReadKey();

        }

        static User GetUser()
        {
            return new User() { Id = 0, Name = "so" };
        }
        static int GetId()
        {
            return 10;
        }
        static string GetName()
        {
            return "so";
        }

        public static Expression StripQuotes(Expression exp)
        {
            while (exp.NodeType == ExpressionType.Quote)
            {
                exp = ((UnaryExpression)exp).Operand;
            }
            return exp;
        }
        public static void InsertRangeTest()
        {
            List<TestEntity> models = new List<TestEntity>();
            for (int i = 0; i < 10000; i++)
            {
                models.Add(new TestEntity()
                {
                    Id = i + 1000,
                    F_Byte = 1,
                    F_Int16 = 1,
                    F_Int32 = i,
                    F_Int64 = i,
                    F_Double = double.MaxValue,
                    F_Float = long.MaxValue,
                    F_Decimal = i,
                    F_Bool = true,
                    F_DateTime = DateTime.Now,
                    F_String = "lu" + i.ToString()
                });
            }

            SW.Do(() =>
            {
                context.InsertRange(models, false);
            });

            object result = null;

            Console.WriteLine(1);
            Console.ReadKey();
        }
        public static void BulkCopyTest()
        {
            List<TestEntity> models = new List<TestEntity>();
            models.Add(new TestEntity()
            {
                Id = 14,
                F_Byte = 1,
                F_Int16 = 1,
                F_Int32 = 1,
                F_Int64 = 1,
                F_Double = 1,
                F_Float = 1,
                F_Decimal = 1,
                F_Bool = true,
                F_DateTime = DateTime.Now,
                F_String = "lu"
            });
            models.Add(new TestEntity()
            {
                Id = 15,
                F_Byte = 1,
                F_Int16 = 1,
                F_Int32 = 1,
                F_Int64 = 1,
                F_Double = 1,
                F_Float = 1,
                F_Decimal = 1,
                F_Bool = true,
                F_DateTime = DateTime.Now,
                F_Guid = Guid.NewGuid(),
                F_String = "so"
            });

            object result = null;

            //SqlConnection conn = new SqlConnection(DbHelper.ConnectionString);
            //conn.BulkCopy1<TestEntity>(models, int.MaxValue, null, null, null);

            context.BeginTransaction();
            result = context.Query<User>().Select(a => new UserLite() { Id = a.Id, Name = a.Name }).ToList();

            context.BulkInsert(models, null, null, false);

            result = context.Query<User>().Select(a => new UserLite() { Id = a.Id, Name = a.Name }).ToList();

            context.CommitTransaction();

            Console.WriteLine(1);
            Console.ReadKey();
        }

        static void SpecifiedTableTest()
        {
            object ret = null;

            User user = null;

            ret = context.Query<User>().Where(a => a.Id > -1).TakePage(1, 20).ToList();
            ret = context.Query<User>("Users_1").Where(a => a.Id > -1).TakePage(1, 20).ToList();

            ret = context.Query<User>().Where(a => a.Id > -1).Select(a => a.Name).TakePage(1, 20).ToList();
            ret = context.Query<User>("Users_1").Where(a => a.Id > -1).Select(a => a.Name).TakePage(1, 20).ToList();


            ret = context.Query<User>().InnerJoin<User>(context.Query<User>("Users_1"), (u1, u2) => u1.Id == u2.Id).Select((u1, u2) => new { u1, u2 }).Where(a => a.u1.Id > -1 && a.u2.Id > -2).ToList();

            user = new User() { Id = 100, Name = "Users_1", Gender = Gender.Man, Age = 180, OpTime = DateTime.Now };
            context.Insert<User>(user);
            context.Insert<User>(user, "Users_1");

            context.Insert<User>(() => new User() { Name = "Users_1_123", Gender = Gender.Woman, Age = 1800, OpTime = DateTime.Now });
            context.Insert<User>(() => new User() { Name = "Users_1_123", Gender = Gender.Woman, Age = 1800, OpTime = DateTime.Now }, "Users_1");

            user = context.Query<User>().Where(a => a.Id == 0).First();
            user.Name = user.Name + "1";
            context.Update(user);
            context.Update(user, "Users_1");

            context.Update<User>(a => a.Id == 0, a => new User() { Age = a.Age + 1 });
            context.Update<User>(a => a.Id == 0, a => new User() { Age = a.Age + 1 }, "Users_1");


            user = new User() { Id = 2114 };
            context.Delete(user);
            context.Delete(user, "Users_1");

            context.Delete<User>(a => a.Id == 2115);
            context.Delete<User>(a => a.Id == 2115, "Users_1");


            ConsoleHelper.WriteLineAndReadKey();
        }
        static void ProcTest()
        {
            object ret = null;

            DbParam id1 = new DbParam("id1", 12);
            DbParam name1 = new DbParam("name1", "shuxin");
            DbParam age = new DbParam("age", null, typeof(int)) { Direction = ParamDirection.Output };
            DbParam name2 = new DbParam("name2", "so", typeof(string)) { Direction = ParamDirection.InputOutput };

            ret = context.SqlQuery<User>("Proc_Test", CommandType.StoredProcedure, id1, name1, age, name2).ToList();

            //ret = context.Session.ExecuteScalar("Proc_Test", CommandType.StoredProcedure, id1, name1, age, name2);
        }

        public static void PredicateTest()
        {
            object ret = null;
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            var q = context.Query<User>();

            List<int> ids = new List<int>();
            ids.Add(1);
            ids.Add(2);
            ids.Add(2);

            string name = "lu";
            string nullString = null;
            //name = null;
            bool b = false;
            bool b1 = true;


            //ret = q.Where(a => true).ToList();
            //ret = q.Where(a => a.Id == FeatureTest.ID).ToList();
            //ret = q.Where(a => a.Id == FeatureTest.ID || a.Id > 1).ToList();
            //ret = q.Where(a => a.Id == 1 && a.Name == name && a.Name == nullString && a.Id == FeatureTest.ID).ToList();
            //ret = q.Where(a => ids.Contains(a.Id)).ToList();
            //ret = q.Where(a => !b == (a.Id > 0)).ToList();

            //ret = q.Where(a => a.Id > 0).Where(a => a.Id == 1).ToList();
            //ret = q.Where(a => !(a.Id > 10)).ToList();
            //ret = q.Where(a => !(a.Name == name)).ToList();
            //ret = q.Where(a => a.Name != name).ToList();
            //ret = q.Where(a => a.Name == name).ToList();

            //ret = q.Where(a => (a.Name == name) == (a.Id > 0)).ToList();

            ret = q.Where(a => a.Name == (a.Name ?? name)).ToList();
            ret = q.Select(a => new { Name = a.Name ?? "abc", Age = a.Age ?? 17, A = a.Age, B = (a.Age ?? 17) < 18 }).ToList();
            //ret = q.Where(a => (a.Age == null ? 0 : 1) == 1).ToList();

            //ret = q.Select(a => b & b1).ToList();
            //ret = q.Select(a => a.Id & 1).ToList();
            //ret = q.Select(a => new { Id = a.Id, And = a.Id & 1, And1 = a.Id & 1 & a.Id, Or = a.Id | 1, B = b & b1 }).ToList();
            //var xxx = q.ToList().Select(a => new { Id = a.Id, And = a.Id & 1, And1 = a.Id & 1 & a.Id, Or = a.Id | 1 }).ToList();

            //ret = q.Where(a => b & true).ToList();
            //ret = q.Where(a => b | true).ToList();
            //ret = q.Where(a => b || true).ToList();

            //ret = q.Where(a => b & b1).ToList();
            //ret = q.Where(a => b | b1).ToList();
            //ret = q.Where(a => b || b1).ToList();

            ret = q.Select(a => a.Name + "-123").ToList();

            Console.WriteLine(1);
            Console.ReadKey();
        }
        //[TestMethod]
        public static void QueryTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);
            context.PagingMode = PagingMode.OFFSET_FETCH;

            IQuery<User> q = context.Query<User>();
            //var s = q.Select(a => (decimal)a.Id).ToString();
            //var xx = q.Select(a => (decimal)a.Id).ToList();

            object ret = null;

            ret = q.Where(a => a.Id > 0).FirstOrDefault();
            ret = q.Where(a => a.Id > 0).Where(a => a.Name.Contains("lu")).ToList();
            ret = q.Where(a => a.Id > 0).OrderBy(a => a.Id).ToList();
            ret = q.Where(a => a.Id > 0).OrderBy(a => a.Id).ThenByDesc(a => a.Age).ToList();
            ret = q.Where(a => a.Id > 0).Skip(1).ToList();
            ret = q.Where(a => a.Id > 0).Take(999).ToList();
            ret = q.Where(a => a.Id > 0).Take(999).OrderBy(a => a.Age).ToList();
            ret = q.Where(a => a.Id > 0).Skip(1).Take(999).ToList();
            ret = q.Where(a => a.Id > 0).OrderBy(a => a.Id).ThenByDesc(a => a.Age).Skip(1).Take(999).ToList();
            ret = q.Where(a => a.Id > 0).OrderBy(a => a.Id).ThenByDesc(a => a.Age).Skip(1).Take(999).Where(a => a.Id > -100).ToList();

            ret = q.Select(a => new { Name1 = a.Name, Name2 = a.Name }).ToList();

            q.Select(a => new { a.Id, a.Name, a.Age }).ToList();
            q.Select(a => new User() { Id = a.Id, Name = a.Name, Age = a.Age }).ToList();


            Console.WriteLine(1);
            Console.ReadKey();
        }
        public static void MethodTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            var q = context.Query<User>();

            var space = new char[] { ' ' };

            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddDays(1);
            int i = 0;
            string n = "";
            //q = q.Where(a => a.Name == n);
            //q = q.Where(a => !string.IsNullOrEmpty(a.Name));
            //q = q.Where(a => bool.Parse(null));
            //var xxx = q.Where(a => a.Name.Substring(0, 2).Length > 2).ToList();
            var xxxx = q.Select(a => new
            {
                Id = a.Id,

                String_Length = (int?)a.Name.Length,
                Substring = a.Name.Substring(0),
                Substring1 = a.Name.Substring(1),
                Substring1_2 = a.Name.Substring(1, 2),
                ToLower = a.Name.ToLower(),
                ToUpper = a.Name.ToUpper(),
                IsNullOrEmpty = string.IsNullOrEmpty(a.Name),
                Contains = (bool?)a.Name.Contains("s"),
                Trim = a.Name.Trim(),
                TrimStart = a.Name.TrimStart(space),
                TrimEnd = a.Name.TrimEnd(space),
                StartsWith = (bool?)a.Name.StartsWith("s"),
                EndsWith = (bool?)a.Name.EndsWith("s"),

                DiffYears = Sql.DiffYears(startTime, endTime),
                DiffMonths = Sql.DiffMonths(startTime, endTime),
                DiffDays = Sql.DiffDays(startTime, endTime),
                DiffHours = Sql.DiffHours(startTime, endTime),
                DiffMinutes = Sql.DiffMinutes(startTime, endTime),
                DiffSeconds = Sql.DiffSeconds(startTime, endTime),
                DiffMilliseconds = Sql.DiffMilliseconds(startTime, endTime),
                //DiffMicroseconds = Sql.DiffMicroseconds(startTime, endTime),//ex

                //No longer support method 'DateTime.Subtract(DateTime d)', instead of using 'Sql.DiffXX'
                //SubtractTotalDays = endTime.Subtract(startTime).TotalDays,
                //SubtractTotalHours = endTime.Subtract(startTime).TotalHours,
                //SubtractTotalMinutes = endTime.Subtract(startTime).TotalMinutes,
                //SubtractTotalSeconds = endTime.Subtract(startTime).TotalSeconds,
                //SubtractTotalMilliseconds = endTime.Subtract(startTime).TotalMilliseconds,

                Now = DateTime.Now,
                UtcNow = DateTime.UtcNow,
                Today = DateTime.Today,
                Date = DateTime.Now.Date,
                Year = DateTime.Now.Year,
                Month = DateTime.Now.Month,
                Day = DateTime.Now.Day,
                Hour = DateTime.Now.Hour,
                Minute = DateTime.Now.Minute,
                Second = DateTime.Now.Second,
                Millisecond = DateTime.Now.Millisecond,
                DayOfWeek = DateTime.Now.DayOfWeek,

                Byte_Parse = byte.Parse("1"),
                Int_Parse = int.Parse("1"),
                Int16_Parse = Int16.Parse("11"),
                Long_Parse = long.Parse("2"),
                Double_Parse = double.Parse("3"),
                Float_Parse = float.Parse("4"),
                //Decimal_Parse = decimal.Parse("5"),
                Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),

                Bool_Parse = bool.Parse("1"),
                DateTime_Parse = DateTime.Parse("2014-1-1"),

                B = a.Age == null ? false : a.Age > 1,
            }).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void SqlQueryTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            object ret = null;
            //Dictionary<string, object> dic = new Dictionary<string, object>();
            //dic.Add("@p", "shuxin");

            var users = context.SqlQuery<User>("select Id as id,Name as name,'asdsd' as Name,ByteArray from Users where Name=@name", DbParam.Create("@name", "lu11"));

            try
            {
                var list = users.ToList();

                ret = context.SqlQuery<int?>("select Id from Users").ToList();
            }
            catch
            {
                ConsoleHelper.WriteLineAndReadKey();
            }

            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void MappingTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);
            IQuery<User> q = context.Query<User>();

            string n = "";
            Expression<Func<User, bool>> p = a => a.Name == n;

            //q.Where(a => a.Name == n);
            var r = q.ToList();

            Console.WriteLine(1);
        }
        public static void JoinQueryTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);
            IQuery<User> q = context.Query<User>();
            IQuery<User> q1 = context.Query<User>();
            IQuery<User> q2 = context.Query<User>();

            object ret = null;
            ret = q.InnerJoin(context.Query<User>(), (a, b) => a.Id == b.Id).Select((a, b) => new { Id1 = a.Id, Id2 = b.Id }).ToList();

            ret = q.InnerJoin(context.Query<User>().Select(a => new { a.Id, a.Name }), (a, b) => a.Id == b.Id).Select((a, b) => new { A = a, B = b }).ToList();

            ret = q.LeftJoin(context.Query<User>().Select(a => new { a.Id, a.Name }), (a, b) => a.Id == b.Id + 1).Select((a, b) => new { A = a, B = b }).ToList();

            ret = q.RightJoin(context.Query<User>().Where(a => a.Id <= 20).Select(a => new { a.Id, a.Name }), (a, b) => a.Id == b.Id + 1).Select((a, b) => new { A = a, B = b }).ToList();

            ret = q.InnerJoin(q1, (a, b) => a.Id == b.Id).InnerJoin(q2, (a, b, c) => a.Name == c.Name).Select((a, b, c) => new { A = a, B = b, C = c }).ToList();

            ret = q.InnerJoin(q1, (a, b) => a.Id == b.Id).InnerJoin(q2, (a, b, c) => a.Name == c.Name).RightJoin(q, (a, b, c, d) => a.Id == d.Id + 1).Select((a, b, c, d) => new { A = a, B = b, C = c, D = d }).ToList();

            ret = q.InnerJoin(q1, (a, b) => a.Id == b.Id).InnerJoin(q2, (a, b, c) => a.Name == c.Name).RightJoin(q, (a, b, c, d) => a.Id == d.Id + 1).Select((a, b, c, d) => new { A = a, D = d }).ToList();

            ret = q.InnerJoin(q1, (a, b) => a.Id == b.Id).InnerJoin(q2.Where(a => a.Id > 0).Select(a => a.Id), (a, b, c) => a.Id == c).RightJoin(q, (a, b, c, d) => a.Id == d.Id + 1).Select((a, b, c, d) => new { a, C = (int?)c }).ToList();

            ret = q.InnerJoin(q1, (a, b) => a.Id == b.Id).LeftJoin(q2.Where(a => a.Id > 0).Select(a => a.Id), (a, b, c) => a.Id == c).FullJoin(q, (a, b, c, d) => a.Id == d.Id + 1).Select((a, b, c, d) => new { a, C = (int?)c }).ToList();

            q.InnerJoin(q1, (a, b) => a.Id == b.Id).Select((a, b) => new { a, b }).Where(a => a.a.Id > 0).ToList();

            Console.WriteLine(1);
        }

        public static void GroupQueryTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            object ret = null;

            var q = context.Query<User>();

            ret = q.Where(a => a.Id > 0).GroupBy(a => a.Id).Select(a => new { a.Id }).ToList();


            var r = q.GroupBy(a => a.Id).Having(a => a.Id > 1).Select(a => new { a.Id, Count = Sql.Count(), Sum = Sql.Sum(a.Id), Max = Sql.Max(a.Id), Min = Sql.Min(a.Id), Avg = Sql.Average(a.Id) }).ToList();

            var r1 = q.GroupBy(a => a.Age).Having(a => Sql.Count() > 0).Select(a => new { a.Age, Count = Sql.Count(), Sum = Sql.Sum(a.Age), Max = Sql.Max(a.Age), Min = Sql.Min(a.Age), Avg = Sql.Average(a.Age) }).ToList();

            var g = q.GroupBy(a => a.Gender);
            //g = g.ThenBy(a => a.Name);
            //g = g.Having(a => a.Id > 0);
            //g = g.Having(a => a.Name.Length > 0);
            var gq = g.Select(a => new { Count = Sql.Count() });

            //gq = gq.Skip(1);
            //gq = gq.Take(100);
            //gq = gq.Where(a => a > -1);

            ret = gq.ToList();
            var c = gq.Count();

            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void AggregateFunctionTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            var q = context.Query<User>();
            //var xxx = q.Select(a => Sql.Count()).ToList().First();


            var xxx = q.Select(a => new { MaxTime = Sql.Max(a.OpTime), MinTime = Sql.Min(a.OpTime) }).ToList();

            q = q.Where(a => a.Id > 0);
            var count = q.Count();
            var longCount = q.LongCount();
            var sum = q.Sum(a => a.Age);
            var max = q.Max(a => a.Age);
            var min = q.Min(a => a.Age);
            var avg = q.Average(a => a.Age);

            decimal dc = 1;

            //Console.WriteLine(q.Sum(a => (decimal)1));

            Console.WriteLine(q.Select(a => Sql.Average(1)).First());
            Console.WriteLine(q.Select(a => Sql.Average(dc)).First());

            //Console.WriteLine(q.Sum(a => 1));
            //Console.WriteLine(q.Sum(a => 1L));
            Console.WriteLine(q.Sum(a => dc));
            //Console.WriteLine(q.Sum(a => 1D));
            //Console.WriteLine(q.Sum(a => 1F));

            //Console.WriteLine(q.Max(a => 1));
            //Console.WriteLine(q.Max(a => 1L));
            Console.WriteLine(q.Max(a => dc));
            //Console.WriteLine(q.Max(a => 1D));
            //Console.WriteLine(q.Max(a => 1F));

            //Console.WriteLine(q.Min(a => 1));
            //Console.WriteLine(q.Min(a => 1L));
            Console.WriteLine(q.Min(a => dc));
            //Console.WriteLine(q.Min(a => 1D));
            //Console.WriteLine(q.Min(a => 1F));

            //Console.WriteLine(q.Average(a => 1));
            //Console.WriteLine(q.Average(a => 1L));
            Console.WriteLine(q.Average(a => dc));
            //Console.WriteLine(q.Average(a => 1D));
            //Console.WriteLine(q.Average(a => 1F));

            ConsoleHelper.WriteLineAndReadKey();
        }

        public static void InsertTest()
        {
            string name = "so88";
            long longAge = 10;
            int? age = 18;
            name = null;
            int r = -1;

            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            User user = new User();
            user.Name = "lu";
            user.Age = 21;
            user.Gender = Gender.Man;
            user.CityId = 1;
            user.OpTime = DateTime.Now;
            user.Id = 100;
            //var id = context.Insert<User>(() => new User() { Name = user.Name, NickName = user.Name, Age = user.Age, Gender = Gender.Man, OpTime = DateTime.Now });

            ////var users = context.Query<User>().Where(a => a.Name == null).ToList();

            ////user = context.Query<User>().Where(a => a.Id == (int)id).First();

            //user.ByteArray = new byte[] { 1, 2, 3 };
            //user.OpTime = DateTime.Now;


            var user1 = context.Insert(user);

            //返回主键 Id
            int id = (int)context.Insert<User>(() => new User() { Name = user.Name, Age = user.Age, Gender = Gender.Man, CityId = 1, OpTime = DateTime.Now });

            ConsoleHelper.WriteLineAndReadKey();
        }

        public static void UpdateTest()
        {
            List<string> names = new List<string>();
            names.Add("so");
            names.Add(null);

            string name = "lu1";
            string stringNull = null;
            int? intNull = null;
            DateTime? dateTimeNull = null;
            //name = null;

            object ret = null;

            int r = -1;

            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            //var u = context.Query<User>().AsTracking().First(a => a.Id == 3);
            //u.Name = u.Name + "1";
            //ret = context.Update(u);
            //u.ByteArray = new byte[] { 1, 2, 3 };
            //u.Name = "lu";
            //ret = context.Update(u);


            r = context.Update<User>(a => a.Id == 100, a => new User() { Name = a.Name, Age = a.Age + 100, Gender = Gender.Man, OpTime = DateTime.Now });

            //r = context.Update<User>(a => new User() { Name = stringNull, NickName = stringNull, Age = intNull, Gender = null, OpTime = dateTimeNull }, a => false);

            User user = new User();
            user.Id = 100;
            user.Name = "shuxin";
            user.Age = 28;
            user.Gender = Gender.Man;
            //user.OpTime = DateTime.Now;

            object o = user;
            r = context.Update(o);

            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void TrackingTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            object ret = null;

            var q = context.Query<User>();
            q = q.AsTracking();

            User user = q.First();
            ret = context.Update(user);

            Console.WriteLine(ret);

            context.TrackEntity(user);
            user.Name = user.Name + "1";
            user.Age = user.Age;
            user.Gender = null;
            ret = context.Update(user);

            Console.WriteLine(ret);

            ret = context.Update(user);
            Console.WriteLine(ret);

            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void DeleteTest()
        {
            string name = "so2";
            //name = null;

            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);
            int r = -1;
            int? age = null;

            //r = context.Delete<User>(a => a.Gender == Gender.Man);
            r = context.Delete<User>(a => a.Age == r);
            //r = context.Delete<User>(a => a.Gender == null);
            //r = context.Delete<User>(a => a.Age == age);
            //r = context.Delete<User>(a => age == a.Age);

            User user = new User();
            user.Id = 6;

            r = context.Delete(user);

            Console.WriteLine(1);
        }




        public static void JTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);
            IQuery<User> users = context.Query<User>();
            IQuery<City> cities = context.Query<City>();
            IQuery<Province> provinces = context.Query<Province>();

            IJoiningQuery<User, City> user_city = users.InnerJoin(cities, (user, city) => user.CityId == city.Id);
            IJoiningQuery<User, City, Province> user_city_province = user_city.InnerJoin(provinces, (user, city, province) => city.ProvinceId == province.Id);

            //只获取UserId,CityName,ProvinceName
            user_city_province.Select((user, city, province) => new { UserId = user.Id, CityName = city.Name, ProvinceName = province.Name }).Where(a => a.UserId == 1).ToList();
            /*
             * SELECT [Users].[Id] AS [UserId],[City].[Name] AS [CityName],[Province].[Name] AS [ProvinceName] FROM [Users] AS [Users] INNER JOIN [City] AS [City] ON [Users].[CityId] = [City].[Id] INNER JOIN [Province] AS [Province] ON [City].[ProvinceId] = [Province].[Id] WHERE [Users].[Id] = 1
             */

            //可以调用 Select 方法返回一个 IQuery<T> 对象
            var view = user_city_province.Select((user, city, province) => new { User = user, City = city, Province = province });

            //查出一个用户及其隶属的城市和省份 
            view.Where(a => a.User.Id == 1).ToList();
            /*
             * SELECT [Users].[Id] AS [Id],[Users].[Name] AS [Name],[Users].[Gender] AS [Gender],[Users].[Age] AS [Age],[Users].[CityId] AS [CityId],[Users].[OpTime] AS [OpTime],[City].[Id] AS [Id0],[City].[Name] AS [Name0],[City].[ProvinceId] AS [ProvinceId],[Province].[Id] AS [Id1],[Province].[Name] AS [Name1] FROM [Users] AS [Users] INNER JOIN [City] AS [City] ON [Users].[CityId] = [City].[Id] INNER JOIN [Province] AS [Province] ON [City].[ProvinceId] = [Province].[Id] WHERE [Users].[Id] = 1
             */

            ////这时候也可以选取指定的字段
            //view.Where(a => a.User.Id == 1).Select(a => new { UserId = a.User.Id, CityName = a.City.Name, ProvinceName = a.Province.Name }).ToList();
            ///*
            // * SELECT [Users].[Id] AS [UserId],[City].[Name] AS [CityName],[Province].[Name] AS [ProvinceName] FROM [Users] AS [Users] INNER JOIN [City] AS [City] ON [Users].[CityId] = [City].[Id] INNER JOIN [Province] AS [Province] ON [City].[ProvinceId] = [Province].[Id] WHERE [Users].[Id] = 1
            // */

            ////假设已经有5个表建立了连接的对象为 jq_q1_q5
            //IJoiningQuery<T1, T2, T3, T4, T5> jq_q1_q5 = null;

            ////jq_q1_q5 调用 Select 方法，返回一个包含 T1-T5 的 IQuery<T> 对象 view_q1_q5
            //var view_q1_q5 = jq_q1_q5.Select((t1, t2, t3, t4, t5) => new { T1 = t1, T2 = t2, T3 = t3, T4 = t4, T5 = t5 });

            ////假设第6个表的 IQuery<T6> 对象为 q6
            //IQuery<T6> q6 = null;

            ////这时，view_q1_q5 与 q6 建立连接，返回 IJoiningQuery 对象 jq
            //var jq = view_q1_q5.InnerJoin(q6, (t1_t5, t6) => t1_t5.T5.XX == t6.XXX);

            ////然后我们调用 jq 的 Select 方法，返回一个包含 T1-T6 的 IQuery<T> 对象 q。
            ////q 又是一个 IQuery<T> 对象，泛型参数为包含 T1-T6 所有信息的匿名对象，拿到它，我们就可以为所欲为了。
            //var q = jq.Select((t1_t5, t6) => new { T1 = t1_t5.T1, T2 = t1_t5.T2, T3 = t1_t5.T3, T4 = t1_t5.T4, T5 = t1_t5.T5, T6 = t6 });

            ////可以直接查出数据库中 T1-T6 的所有信息
            //q.ToList();

            ////也可以选取 T1-T6 中我们想要的字段
            //q.Select(a => new { a.T1.xx, a.T2.xx, a.T3.xx /*...*/}).ToList();


            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void AFTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);
            IQuery<User> q = context.Query<User>();

            q.Select(a => Sql.Count()).First();
            /*
             * SELECT TOP (1) COUNT(1) AS [C] FROM [Users] AS [Users]
             */

            q.Select(a => new { Count = Sql.Count(), LongCount = Sql.LongCount(), Sum = Sql.Sum(a.Age), Max = Sql.Max(a.Age), Min = Sql.Min(a.Age), Average = Sql.Average(a.Age) }).First();
            /*
             * SELECT TOP (1) COUNT(1) AS [Count],COUNT_BIG(1) AS [LongCount],SUM([Users].[Age]) AS [Sum],MAX([Users].[Age]) AS [Max],MIN([Users].[Age]) AS [Min],CAST(AVG([Users].[Age]) AS FLOAT) AS [Average] FROM [Users] AS [Users]
             */

            var count = q.Count();
            /*
             * SELECT COUNT(1) AS [C] FROM [Users] AS [Users]
             */

            var longCount = q.LongCount();
            /*
             * SELECT COUNT_BIG(1) AS [C] FROM [Users] AS [Users]
             */

            var sum = q.Sum(a => a.Age);
            /*
             * SELECT SUM([Users].[Age]) AS [C] FROM [Users] AS [Users]
             */

            var max = q.Max(a => a.Age);
            /*
             * SELECT MAX([Users].[Age]) AS [C] FROM [Users] AS [Users]
             */

            var min = q.Min(a => a.Age);
            /*
             * SELECT MIN([Users].[Age]) AS [C] FROM [Users] AS [Users]
             */

            var avg = q.Average(a => a.Age);
            /*
             * SELECT CAST(AVG([Users].[Age]) AS FLOAT) AS [C] FROM [Users] AS [Users]
             */

            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void GQTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            IQuery<User> q = context.Query<User>();

            IGroupingQuery<User> g = q.Where(a => a.Id > 0).GroupBy(a => a.Age);
            g = g.Having(a => a.Age > 1 && Sql.Count() > 0);

            g.Select(a => new { a.Age, Count = Sql.Count(), Sum = Sql.Sum(a.Age), Max = Sql.Max(a.Age), Min = Sql.Min(a.Age), Avg = Sql.Average(a.Age) }).ToList();
            /*
             * SELECT [Users].[Age] AS [Age],COUNT(1) AS [Count],SUM([Users].[Age]) AS [Sum],MAX([Users].[Age]) AS [Max],MIN([Users].[Age]) AS [Min],CAST(AVG([Users].[Age]) AS FLOAT) AS [Avg] FROM [Users] AS [Users] WHERE [Users].[Id] > 0 GROUP BY [Users].[Age] HAVING ([Users].[Age] > 1 AND COUNT(1) > 0)
             */



        }

        public static void GroupJion()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            IQuery<User> users = context.Query<User>();
            IQuery<City> cities = context.Query<City>();
            var gq = users.GroupBy(a => a.CityId).Select(a => new { a.CityId, MinAge = Sql.Min(a.Age) });

            cities.LeftJoin(gq, (city, g) => city.Id == g.CityId).Select((city, g) => new { City = city, MinAge = g.MinAge }).ToList();
            /*
             * SELECT [T].[MinAge] AS [MinAge],[City].[Id] AS [Id],[City].[Name] AS [Name],[City].[ProvinceId] AS [ProvinceId] FROM [City] AS [City] LEFT JOIN (SELECT [Users].[CityId] AS [CityId],MIN([Users].[Age]) AS [MinAge] FROM [Users] AS [Users] GROUP BY [Users].[CityId]) AS [T] ON [City].[Id] = [T].[CityId]
             */

            ConsoleHelper.WriteLineAndReadKey();
        }

        public static void Predicate()
        {
            object ret = null;
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            IQuery<User> q = context.Query<User>();

            List<int> ids = new List<int>();
            ids.Add(1);
            ids.Add(2);
            ids.Add(2);

            string name = "lu";
            string nullString = null;
            bool b = false;
            bool b1 = true;

            q.Where(a => true).ToList();
            q.Where(a => a.Id == 1).ToList();
            q.Where(a => a.Id == 1 || a.Id > 1).ToList();
            q.Where(a => a.Id == 1 && a.Name == name && a.Name == nullString && a.Id == FeatureTest.ID).ToList();
            q.Where(a => ids.Contains(a.Id)).ToList();
            q.Where(a => !b == (a.Id > 0)).ToList();
            q.Where(a => a.Id > 0).Where(a => a.Id == 1).ToList();
            q.Where(a => !(a.Id > 10)).ToList();
            q.Where(a => !(a.Name == name)).ToList();
            q.Where(a => a.Name != name).ToList();
            q.Where(a => a.Name == name).ToList();
            q.Where(a => (a.Name == name) == (a.Id > 0)).ToList();
            q.Where(a => a.Name == (a.Name ?? name)).ToList();
            q.Where(a => (a.Age == null ? 0 : 1) == 1).ToList();

            //运算操作符
            q.Select(a => new
            {
                Add = 1 + 2,
                Subtract = 2 - 1,
                Multiply = 2 * 11,
                Divide = 4 / 2,
                And = true & false,
                IntAnd = 1 & 2,
                Or = true | false,
                IntOr = 3 | 1,
            }).ToList();


            q.Select(a => b & b1).ToList();
            q.Select(a => a.Id & 1).ToList();
            q.Select(a => new { Id = a.Id, And = a.Id & 1, And1 = a.Id & 1 & a.Id, Or = a.Id | 1, B = b & b1 }).ToList();

            ret = q.Where(a => b & true).ToList();
            ret = q.Where(a => b | true).ToList();
            ret = q.Where(a => b || true).ToList();

            ret = q.Where(a => b & b1).ToList();
            ret = q.Where(a => b | b1).ToList();
            ret = q.Where(a => b || b1).ToList();

            Console.WriteLine(1);
            Console.ReadKey();
        }
        public static void MTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            IQuery<User> q = context.Query<User>();

            var space = new char[] { ' ' };

            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddDays(1);
            q.Select(a => new
            {
                Id = a.Id,

                String_Length = (int?)a.Name.Length,//LEN([Users].[Name])
                Substring = a.Name.Substring(0),//SUBSTRING([Users].[Name],0 + 1,LEN([Users].[Name]))
                Substring1 = a.Name.Substring(1),//SUBSTRING([Users].[Name],1 + 1,LEN([Users].[Name]))
                Substring1_2 = a.Name.Substring(1, 2),//SUBSTRING([Users].[Name],1 + 1,2)
                ToLower = a.Name.ToLower(),//LOWER([Users].[Name])
                ToUpper = a.Name.ToUpper(),//UPPER([Users].[Name])
                IsNullOrEmpty = string.IsNullOrEmpty(a.Name),//太长，不贴了
                Contains = (bool?)a.Name.Contains("s"),//太长，略
                Trim = a.Name.Trim(),//RTRIM(LTRIM([Users].[Name]))
                TrimStart = a.Name.TrimStart(space),//LTRIM([Users].[Name])
                TrimEnd = a.Name.TrimEnd(space),//RTRIM([Users].[Name])
                StartsWith = (bool?)a.Name.StartsWith("s"),//太长，略
                EndsWith = (bool?)a.Name.EndsWith("s"),//太长，略

                SubtractTotalDays = endTime.Subtract(startTime).TotalDays,//CAST(DATEDIFF(DAY,@P_0,@P_1)
                SubtractTotalHours = endTime.Subtract(startTime).TotalHours,//CAST(DATEDIFF(HOUR,@P_0,@P_1)
                SubtractTotalMinutes = endTime.Subtract(startTime).TotalMinutes,//CAST(DATEDIFF(MINUTE,@P_0,@P_1)
                SubtractTotalSeconds = endTime.Subtract(startTime).TotalSeconds,//CAST(DATEDIFF(SECOND,@P_0,@P_1)
                SubtractTotalMilliseconds = endTime.Subtract(startTime).TotalMilliseconds,//CAST(DATEDIFF(MILLISECOND,@P_0,@P_1)

                Now = DateTime.Now,//GETDATE()
                UtcNow = DateTime.UtcNow,//GETUTCDATE()
                Today = DateTime.Today,//CAST(GETDATE() AS DATE)
                Date = DateTime.Now.Date,//CAST(GETDATE() AS DATE)
                Year = DateTime.Now.Year,//DATEPART(YEAR,GETDATE())
                Month = DateTime.Now.Month,//DATEPART(MONTH,GETDATE())
                Day = DateTime.Now.Day,//DATEPART(DAY,GETDATE())
                Hour = DateTime.Now.Hour,//DATEPART(HOUR,GETDATE())
                Minute = DateTime.Now.Minute,//DATEPART(MINUTE,GETDATE())
                Second = DateTime.Now.Second,//DATEPART(SECOND,GETDATE())
                Millisecond = DateTime.Now.Millisecond,//DATEPART(MILLISECOND,GETDATE())
                DayOfWeek = DateTime.Now.DayOfWeek,//(DATEPART(WEEKDAY,GETDATE()) - 1)

                Int_Parse = int.Parse("1"),//CAST(N'1' AS INT)
                Int16_Parse = Int16.Parse("11"),//CAST(N'11' AS SMALLINT)
                Long_Parse = long.Parse("2"),//CAST(N'2' AS BIGINT)
                Double_Parse = double.Parse("3"),//CAST(N'3' AS FLOAT)
                Float_Parse = float.Parse("4"),//CAST(N'4' AS REAL)
                Decimal_Parse = decimal.Parse("5"),//CAST(N'5' AS DECIMAL)
                Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),//CAST(N'xxx' AS UNIQUEIDENTIFIER) AS [Guid_Parse]

                Bool_Parse = bool.Parse("1"),//CASE WHEN CAST(N'1' AS BIT) = CAST(1 AS BIT) THEN CAST(1 AS BIT) WHEN NOT (CAST(N'1' AS BIT) = CAST(1 AS BIT)) THEN CAST(0 AS BIT) ELSE NULL END AS [Bool_Parse]
                DateTime_Parse = DateTime.Parse("1992-1-16"),//CAST(N'1992-1-16' AS DATETIME) AS [DateTime_Parse]

                B = a.Age == null ? false : a.Age > 1,
            }).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void QTest()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            IQuery<User> q = context.Query<User>();
            q.Where(a => a.Id > 0).FirstOrDefault();
            q.Where(a => a.Id > 0).ToList();
            q.Where(a => a.Id > 0).OrderBy(a => a.Age).ToList();
            q.Where(a => a.Id > 0).Take(999).OrderBy(a => a.Age).ToList();

            //分页，避免生成的 sql 语句太长，占篇幅，只选取 Id 和 Name 两个字段
            q.Where(a => a.Id > 0).OrderBy(a => a.Age).ThenByDesc(a => a.Id).Select(a => new { a.Id, a.Name }).Skip(1).Take(999).ToList();
            /*
             * SELECT TOP (999) [T].[Id] AS [Id],[T].[Name] AS [Name] FROM (SELECT [Users].[Id] AS [Id],[Users].[Name] AS [Name],ROW_NUMBER() OVER(ORDER BY [Users].[Age] ASC,[Users].[Id] DESC) AS [ROW_NUMBER_0] FROM [Users] AS [Users] WHERE [Users].[Id] > 0) AS [T] WHERE [T].[ROW_NUMBER_0] > 1
             */

            //如果需要多个条件的话
            q.Where(a => a.Id > 0).Where(a => a.Name.Contains("lu")).ToList();
            /*
             * SELECT [Users].[Id] AS [Id],[Users].[Name] AS [Name],[Users].[Gender] AS [Gender],[Users].[Age] AS [Age],[Users].[CityId] AS [CityId],[Users].[OpTime] AS [OpTime] FROM [Users] AS [Users] WHERE ([Users].[Id] > 0 AND [Users].[Name] LIKE '%' + N'lu' + '%')
             */

            //选取指定字段
            q.Select(a => new { a.Id, a.Name, a.Age }).ToList();
            //或者
            q.Select(a => new User() { Id = a.Id, Name = a.Name, Age = a.Age }).ToList();
            /*
             * SELECT [Users].[Id] AS [Id],[Users].[Name] AS [Name],[Users].[Age] AS [Age] FROM [Users] AS [Users]
             */

            ConsoleHelper.WriteLineAndReadKey();
        }


        public static void InsertTest1()
        {
            IDbContext context = new MsSqlContext(DbHelper.ConnectionString);

            //返回主键 Id
            int id = (int)context.Insert<User>(() => new User() { Name = "lu", Age = 18, Gender = Gender.Man, CityId = 1, OpTime = DateTime.Now });
            /*
             * INSERT INTO [Users]([Name],[Age],[Gender],[CityId],[OpTime]) VALUES(N'lu',18,1,1,GETDATE());SELECT @@IDENTITY
             */

            User user = new User();
            user.Name = "lu";
            //user.Age = 18;
            user.Gender = Gender.Man;
            user.CityId = 1;
            user.OpTime = DateTime.Now;

            //会自动将自增 Id 设置到 user 的 Id 属性上
            user = context.Insert(user);
            /*
             * String @P_0 = "lu";
               Gender @P_1 = Man;
               Int32 @P_2 = 18;
               Int32 @P_3 = 1;
               DateTime @P_4 = "1992/1/16 0:00:00";
               INSERT INTO [Users]([Name],[Gender],[Age],[CityId],[OpTime]) VALUES(@P_0,@P_1,@P_2,@P_3,@P_4);SELECT @@IDENTITY
             */


            ConsoleHelper.WriteLineAndReadKey();
        }

        public static void UpdateTest1()
        {
            object ret = null;

            int r = -1;

            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            var u = context.Query<User>().AsTracking().First(a => a.Id == 3);

            context.Update<User>(a => a.Id == 1, a => new User() { Name = a.Name, Age = a.Age + 100, Gender = Gender.Man, OpTime = DateTime.Now });
            /*
             * UPDATE [Users] SET [Name]=[Users].[Name],[Age]=([Users].[Age] + 100),[Gender]=1,[OpTime]=GETDATE() WHERE [Users].[Id] = 1
             */

            //批量更新
            //给所有女性年轻 10 岁
            context.Update<User>(a => a.Gender == Gender.Woman, a => new User() { Age = a.Age - 10, OpTime = DateTime.Now });
            /*
             * UPDATE [Users] SET [Age]=([Users].[Age] - 10),[OpTime]=GETDATE() WHERE [Users].[Gender] = 2
             */

            User user = new User();
            user.Id = 1;
            user.Name = "lu";
            user.Age = 28;
            user.Gender = Gender.Man;
            user.OpTime = DateTime.Now;

            context.Update(user); //会更新所有映射的字段
            /*
             * String @P_0 = "lu";
               Gender @P_1 = Man;
               Int32 @P_2 = 28;
               Nullable<Int32> @P_3 = NULL;
               DateTime @P_4 = "2016/7/8 11:28:27";
               Int32 @P_5 = 1;
               UPDATE [Users] SET [Name]=@P_0,[Gender]=@P_1,[Age]=@P_2,[CityId]=@P_3,[OpTime]=@P_4 WHERE [Users].[Id] = @P_5
             */

            /*
             * 支持只更新属性值已变的属性
             */

            context.TrackEntity(user);//在上下文中跟踪实体
            user.Name = user.Name + "1";
            context.Update(user);//这时只会更新被修改的字段
            /*
             * String @P_0 = "lu1";
               Int32 @P_1 = 1;
               UPDATE [Users] SET [Name]=@P_0 WHERE [Users].[Id] = @P_1
             */


            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void DeleteTest1()
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            context.Delete<User>(a => a.Id == 1);
            /*
             * DELETE [Users] WHERE [Users].[Id] = 1
             */

            //批量删除
            //删除所有不男不女的用户
            context.Delete<User>(a => a.Gender == null);
            /*
             * DELETE [Users] WHERE [Users].[Gender] IS NULL
             */

            User user = new User();
            user.Id = 1;
            context.Delete(user);
            /*
             * Int32 @P_0 = 1;
               DELETE [Users] WHERE [Users].[Id] = @P_0
             */

            IDbSession dbSession = context.Session;

            try
            {
                dbSession.BeginTransaction();

                //to do somethings here...

                dbSession.CommitTransaction();
            }
            catch
            {
                dbSession.RollbackTransaction();
            }

            ConsoleHelper.WriteLineAndReadKey(1);
        }


    }

}
