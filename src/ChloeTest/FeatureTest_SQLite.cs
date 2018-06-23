using Chloe;
using Chloe.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChloeTest
{
    public class FeatureTest_SQLite
    {
        //static string ConnString = "Data Source=D:\\MyProject\\Chloe.db;Version=3;Pooling=True;Max Pool Size=100;";
        static string ConnString = "Data Source=D:\\MyProject\\Chloe.db;";
        static SQLiteContext context = new SQLiteContext(new SQLiteConnectionFactory(ConnString));
        static readonly int ID = 9999;

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

            string name = "lu";
            string name1 = "lu";
            string nameNull = null;
            string ageString = "18";
            Int16 shortId = 2;
            int id = 2;
            long longId = 2;
            Gender gender = Gender.Man;

            result = q.Where(a => a.OpTime != null).Select(a => a.OpTime.Value.AddDays(id)).ToList();


            ConsoleHelper.WriteLineAndReadKey();
        }

        static User GetUser()
        {
            return new User() { Id = 1, Name = "so" };
        }
        static int GetId()
        {
            return 10;
        }
        static string GetName()
        {
            return "so";
        }

        public static void InsertRangeTest()
        {
            List<TestEntity> models = new List<TestEntity>();
            for (int i = 0; i < 10000; i++)
            {
                models.Add(new TestEntity()
                {
                    Id = i + 100,
                    F_Byte = 1,
                    F_Int16 = 1,
                    F_Int32 = i,
                    F_Int64 = i,
                    F_Double = i,
                    F_Float = i,
                    F_Decimal = i,
                    F_Bool = true,
                    F_DateTime = DateTime.Now,
                    F_Guid = Guid.NewGuid(),
                    F_String = "lu" + i.ToString()
                });
            }

            //for (int i = 0; i < 5; i++)
            //{
            //    context.BeginTransaction();
            //    SW.Do(() =>
            //    {
            //        foreach (var item in models)
            //        {
            //            context.Insert(item);
            //        }
            //    });
            //    context.CommitTransaction();
            //}

            for (int i = 0; i < 1; i++)
            {
                SW.Do(() =>
                {
                    context.InsertRange(models, true);
                });
            }


            object result = null;

            //result = context.Query<TestEntity>().OrderByDesc(a => a.Id).ToList();

            Console.WriteLine(1);
            Console.ReadKey();
        }


        static int finishThreadCount = 0;
        static void LockTest()
        {
            Console.WriteLine("start...");
            //context.BeginTransaction(IsolationLevel.Unspecified);
            //ThreadPool.SetMinThreads(20, 4);
            for (int i = 0; i < 100; i++)
            {
                ThreadPool.QueueUserWorkItem((state) => { LockTestFn(); });
            }


            //Console.WriteLine("start...");
            Console.ReadKey();
            LockTestFn();
            Console.WriteLine(string.Format("finishThreadCount:{0}", finishThreadCount));
            Console.ReadKey();

        }
        static void LockTestFn()
        {
            SQLiteContext context = new SQLiteContext(new SQLiteConnectionFactory(ConnString));

            try
            {
                for (int i = 0; i < 2; i++)
                {
                    context.BeginTransaction(IsolationLevel.Unspecified);
                    try
                    {
                        var users = context.Query<User>().ToList();
                        context.Update<User>(a => a.Id == 12, a => new User() { Age = a.Age + 1 });
                        context.Query<User>().ToList();
                        context.Query<User>().ToList();
                        context.Update<User>(a => a.Id == 12, a => new User() { Age = a.Age + 1 });
                        context.Update<User>(a => a.Id == 12, a => new User() { Age = a.Age + 1 });

                        //if (i == 1)
                        //    throw new Exception("error");

                        context.CommitTransaction();
                    }
                    catch
                    {
                        context.RollbackTransaction();
                        throw;
                    }
                }

                Console.WriteLine(string.Format("执行完毕：{0}", Thread.CurrentThread.ManagedThreadId.ToString()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.WriteLine(string.Format("执行出错：{0}", Thread.CurrentThread.ManagedThreadId.ToString()));
                return;
            }

            int ret = System.Threading.Interlocked.Increment(ref finishThreadCount);
            Console.WriteLine(string.Format("finishThreadCount:{0}", ret));
        }

        public static void PredicateTest()
        {
            object ret = null;

            var q = context.Query<User>();

            List<int> ids = new List<int>();
            ids.Add(1);
            ids.Add(2);

            string name = "lu";
            string nullString = null;
            //name = null;
            bool b = false;
            bool b1 = true;


            ret = q.Where(a => true).ToList();
            ret = q.Where(a => a.Id == ID).ToList();
            ret = q.Where(a => a.Id == ID || a.Id > 1).ToList();
            ret = q.Where(a => a.Id == 1 && a.Name == name && a.Name == nullString && a.Id == ID).ToList();
            ret = q.Where(a => ids.Contains(a.Id)).ToList();
            ret = q.Where(a => !b == (a.Id > 0)).ToList();

            ret = q.Where(a => a.Id > 0).Where(a => a.Id == 1).ToList();
            ret = q.Where(a => !(a.Id > 10)).ToList();
            ret = q.Where(a => !(a.Name == name)).ToList();
            ret = q.Where(a => a.Name != name).ToList();
            ret = q.Where(a => a.Name == name).ToList();

            ret = q.Where(a => (a.Name == name) == (a.Id > 0)).ToList();

            ret = q.Where(a => a.Name == (a.Name ?? name)).ToList();
            ret = q.Where(a => (a.Age == null ? 0 : 1) == 1).ToList();

            ret = q.Select(a => b & b1).ToList();
            ret = q.Select(a => a.Id & 1).ToList();
            ret = q.Select(a => new { Id = a.Id, And = a.Id & 1, And1 = a.Id & 1 & a.Id, Or = a.Id | 1, B = b & b1 }).ToList();
            var xxx = q.ToList().Select(a => new { Id = a.Id, And = a.Id & 1, And1 = a.Id & 1 & a.Id, Or = a.Id | 1 }).ToList();

            ret = q.Where(a => b & true).ToList();
            ret = q.Where(a => b | true).ToList();
            ret = q.Where(a => b || true).ToList();

            ret = q.Where(a => b & b1).ToList();
            ret = q.Where(a => b | b1).ToList();
            ret = q.Where(a => b || b1).ToList();

            ret = q.Select(a => a.Name + "-123").ToList();

            Console.WriteLine(1);
            Console.ReadKey();
        }
        //[TestMethod]
        public static void QueryTest()
        {
            IQuery<User> q = context.Query<User>();
            var s = q.Select(a => (decimal)a.Id).ToString();
            var xx = q.Select(a => (decimal)a.Id).ToList();

            object ret = null;
            string name = "s";
            ret = q.Where(a => a.Id > 0).FirstOrDefault();
            ret = q.Where(a => a.Id > 0).Where(a => a.Name.Contains("lu")).ToList();
            ret = q.Where(a => a.Id > 0).Where(a => a.Name.Contains(name)).ToList();
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
            var q = context.Query<User>();

            var space = new char[] { ' ' };

            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddYears(1).AddMonths(1).AddDays(1);

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
                //DiffMilliseconds = Sql.DiffMilliseconds(startTime, endTime),
                //DiffMicroseconds = Sql.DiffMicroseconds(startTime, endTime),//ex

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

                Int_Parse = int.Parse("1"),
                Int16_Parse = Int16.Parse("11"),
                Long_Parse = long.Parse("2"),
                Double_Parse = double.Parse("3"),
                Float_Parse = float.Parse("4"),
                //Decimal_Parse = decimal.Parse("5"),
                Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),

                Bool_Parse = bool.Parse("1"),
                DateTime_Parse = DateTime.Parse("2016-08-07"),

                B = a.Age == null ? false : a.Age > 1,
            }).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void SqlQueryTest()
        {
            object ret = null;
            //Dictionary<string, object> dic = new Dictionary<string, object>();
            //dic.Add("@p", "shuxin");

            var users = context.SqlQuery<User>("select Id as id,Name as name,'asdsd' as Name from Users where Name=@name", DbParam.Create("@name", "so"));

            var list = users.ToList();

            ret = context.SqlQuery<int?>("select Id from Users").ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void MappingTest()
        {
            IQuery<User> q = context.Query<User>();

            string n = "";
            Expression<Func<User, bool>> p = a => a.Name == n;

            //q.Where(a => a.Name == n);
            var r = q.ToList();

            Console.WriteLine(1);
        }
        public static void JoinQueryTest()
        {
            IQuery<User> q = context.Query<User>();
            IQuery<User> q1 = context.Query<User>();
            IQuery<User> q2 = context.Query<User>();

            object ret = null;
            ret = q.InnerJoin(context.Query<User>(), (a, b) => a.Id == b.Id).Select((a, b) => new { A = a, B = b }).ToList();

            ret = q.InnerJoin(context.Query<User>().Select(a => new { a.Id, a.Name }), (a, b) => a.Id == b.Id).Select((a, b) => new { A = a, B = b }).ToList();

            ret = q.LeftJoin(context.Query<User>().Select(a => new { a.Id, a.Name }), (a, b) => a.Id == b.Id + 1).Select((a, b) => new { A = a, B = b }).ToList();

            //ret = q.RightJoin(context.Query<User>().Where(a => a.Id <= 20).Select(a => new { a.Id, a.Name }), (a, b) => a.Id == b.Id + 1).Select((a, b) => new { A = a, B = b }).ToList();

            ret = q.InnerJoin(q1, (a, b) => a.Id == b.Id).InnerJoin(q2, (a, b, c) => a.Name == c.Name).Select((a, b, c) => new { A = a, B = b, C = c }).ToList();

            //ret = q.InnerJoin(q1, (a, b) => a.Id == b.Id).InnerJoin(q2, (a, b, c) => a.Name == c.Name).RightJoin(q, (a, b, c, d) => a.Id == d.Id + 1).Select((a, b, c, d) => new { A = a, B = b, C = c, D = d }).ToList();

            //ret = q.InnerJoin(q1, (a, b) => a.Id == b.Id).InnerJoin(q2, (a, b, c) => a.Name == c.Name).RightJoin(q, (a, b, c, d) => a.Id == d.Id + 1).Select((a, b, c, d) => new { A = a, D = d }).ToList();

            //ret = q.InnerJoin(q1, (a, b) => a.Id == b.Id).InnerJoin(q2.Where(a => a.Id > 0).Select(a => a.Id), (a, b, c) => a.Id == c).RightJoin(q, (a, b, c, d) => a.Id == d.Id + 1).Select((a, b, c, d) => new { a, C = (int?)c }).ToList();

            q.InnerJoin(q1, (a, b) => a.Id == b.Id).Select((a, b) => new { a, b }).Where(a => a.a.Id > 0).ToList();

            Console.WriteLine(1);
        }

        public static void GroupQueryTest()
        {
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
            var q = context.Query<User>();
            var xxx = q.Select(a => Sql.Count()).ToList().First();

            q = q.Where(a => a.Id > 0);
            var count = q.Count();
            var longCount = q.LongCount();
            var sum = q.Sum(a => a.Age);
            var max = q.Max(a => a.Age);
            var min = q.Min(a => a.Age);
            var avg = q.Average(a => a.Age);



            decimal dc = 1;

            Console.WriteLine(q.Select(a => Sql.Average(1)).First());
            Console.WriteLine(q.Select(a => Sql.Average(dc)).First());

            Console.WriteLine(q.Sum(a => 1));
            Console.WriteLine(q.Sum(a => 1L));
            Console.WriteLine(q.Sum(a => dc));
            Console.WriteLine(q.Sum(a => 1D));
            Console.WriteLine(q.Sum(a => 1F));

            Console.WriteLine(q.Max(a => 1));
            Console.WriteLine(q.Max(a => 1L));
            Console.WriteLine(q.Max(a => dc));
            Console.WriteLine(q.Max(a => 1D));
            Console.WriteLine(q.Max(a => 1F));

            Console.WriteLine(q.Min(a => 1));
            Console.WriteLine(q.Min(a => 1L));
            Console.WriteLine(q.Min(a => dc));
            Console.WriteLine(q.Min(a => 1D));
            Console.WriteLine(q.Min(a => 1F));

            Console.WriteLine(q.Average(a => 1));
            Console.WriteLine(q.Average(a => 1L));
            Console.WriteLine(q.Average(a => dc));
            Console.WriteLine(q.Average(a => 1D));
            Console.WriteLine(q.Average(a => 1F));

            Console.WriteLine(1);
        }

        public static void InsertTest()
        {
            string name = "so88";
            long longAge = 10;
            int? age = 18;
            name = null;
            int r = -1;


            User user = new User();
            user.Name = "lu";
            user.Age = 21;
            user.Gender = Gender.Man;
            user.CityId = 1;
            user.OpTime = DateTime.Now;

            var id = context.Insert<User>(() => new User() { Name = user.Name, Age = user.Age, Gender = Gender.Man, OpTime = DateTime.Now });

            ////var users = context.Query<User>().Where(a => a.Name == null).ToList();

            ////user = context.Query<User>().Where(a => a.Id == (int)id).First();

            //user.ByteArray = new byte[] { 1, 2, 3 };
            //user.OpTime = DateTime.Now;


            var user1 = context.Insert(user);

            //返回主键 Id
            //int id = (int)context.Insert<User>(() => new User() { Name = user.Name, Age = user.Age, Gender = Gender.Man, CityId = 1, OpTime = DateTime.Now });

            TestEntity te = new TestEntity();
            te.F_Byte = 1;
            te.F_Int16 = 1;
            te.F_Int32 = 32;
            te.F_Int64 = 64;
            te.F_Double = 1.1234;
            te.F_Float = 1.12F;
            te.F_Decimal = 1.1234M;
            te.F_Bool = false;
            te.F_DateTime = DateTime.Now;
            te.F_Guid = Guid.NewGuid();
            te.F_String = "lu";

            context.Insert(te);

            var ret = context.Query<TestEntity>().ToList();

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


            var u = context.Query<User>().AsTracking().First(a => a.Id == 3);
            u.Name = u.Name + "1";
            ret = context.Update(u);
            u.ByteArray = new byte[] { 1, 2, 3 };
            u.Name = "lu";
            ret = context.Update(u);


            r = context.Update<User>(a => a.Name == name, a => new User() { Name = a.Name, Age = a.Age + 100, Gender = Gender.Man, OpTime = DateTime.Now });

            r = context.Update<User>(a => false, a => new User() { Name = stringNull, Age = intNull, Gender = null });

            User user = new User();
            user.Id = 2;
            user.Name = "shuxin";
            user.Age = 28;
            user.Gender = Gender.Man;
            user.OpTime = DateTime.Now;

            object o = user;
            r = context.Update(o);

            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void TrackingTest()
        {
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

            int r = -1;
            int? age = null;

            context.Session.BeginTransaction();

            //r = context.Delete<User>(a => a.Gender == Gender.Man);
            r = context.Delete<User>(a => a.Age == 28);
            //r = context.Delete<User>(a => a.Gender == null);
            //r = context.Delete<User>(a => a.Age == age);
            //r = context.Delete<User>(a => age == a.Age);

            User user = new User();
            user.Id = 16;

            r = context.Delete(user);

            context.Session.RollbackTransaction();

            Console.WriteLine(1);
        }




        public static void BaseQuery()
        {
            IQuery<User> q = context.Query<User>();

            q.Where(a => a.Id == 1).FirstOrDefault();
            /*
             * SELECT `Users`.`Id` AS `Id`,`Users`.`Name` AS `Name`,`Users`.`Gender` AS `Gender`,`Users`.`Age` AS `Age`,`Users`.`CityId` AS `CityId`,`Users`.`OpTime` AS `OpTime` FROM `Users` AS `Users` WHERE `Users`.`Id` = 1 LIMIT 0,1
             */

            //可以选取指定的字段
            q.Where(a => a.Id == 1).Select(a => new { a.Id, a.Name }).FirstOrDefault();
            /*
             * SELECT `Users`.`Id` AS `Id`,`Users`.`Name` AS `Name` FROM `Users` AS `Users` WHERE `Users`.`Id` = 1 LIMIT 0,1
             */

            //分页
            q.Where(a => a.Id > 0).OrderBy(a => a.Age).Skip(1).Take(999).ToList();
            /*
             * SELECT `Users`.`Id` AS `Id`,`Users`.`Name` AS `Name`,`Users`.`Gender` AS `Gender`,`Users`.`Age` AS `Age`,`Users`.`CityId` AS `CityId`,`Users`.`OpTime` AS `OpTime` FROM `Users` AS `Users` WHERE `Users`.`Id` > 0 ORDER BY `Users`.`Age` ASC LIMIT 1,999
             */

            ConsoleHelper.WriteLineAndReadKey();
        }

        public static void JoinQuery()
        {
            IQuery<User> users = context.Query<User>();
            IQuery<City> cities = context.Query<City>();
            IQuery<Province> provinces = context.Query<Province>();

            //建立连接
            IJoiningQuery<User, City> user_city = users.InnerJoin(cities, (user, city) => user.CityId == city.Id);
            IJoiningQuery<User, City, Province> user_city_province = user_city.InnerJoin(provinces, (user, city, province) => city.ProvinceId == province.Id);

            //查出一个用户及其隶属的城市和省份的所有信息
            var view = user_city_province.Select((user, city, province) => new { User = user, City = city, Province = province }).Where(a => a.User.Id == 1).ToList();
            /*
             * SELECT `Users`.`Id` AS `Id`,`Users`.`Name` AS `Name`,`Users`.`Gender` AS `Gender`,`Users`.`Age` AS `Age`,`Users`.`CityId` AS `CityId`,`Users`.`OpTime` AS `OpTime`,`City`.`Id` AS `Id0`,`City`.`Name` AS `Name0`,`City`.`ProvinceId` AS `ProvinceId`,`Province`.`Id` AS `Id1`,`Province`.`Name` AS `Name1` FROM `Users` AS `Users` INNER JOIN `City` AS `City` ON `Users`.`CityId` = `City`.`Id` INNER JOIN `Province` AS `Province` ON `City`.`ProvinceId` = `Province`.`Id` WHERE `Users`.`Id` = 1
             */

            //也可以只获取指定的字段信息：UserId,UserName,CityName,ProvinceName
            user_city_province.Select((user, city, province) => new { UserId = user.Id, UserName = user.Name, CityName = city.Name, ProvinceName = province.Name }).Where(a => a.UserId == 1).ToList();
            /*
             * SELECT `Users`.`Id` AS `UserId`,`Users`.`Name` AS `UserName`,`City`.`Name` AS `CityName`,`Province`.`Name` AS `ProvinceName` FROM `Users` AS `Users` INNER JOIN `City` AS `City` ON `Users`.`CityId` = `City`.`Id` INNER JOIN `Province` AS `Province` ON `City`.`ProvinceId` = `Province`.`Id` WHERE `Users`.`Id` = 1
             */


            ConsoleHelper.WriteLineAndReadKey();
        }

        public static void AFQuery()
        {
            IQuery<User> q = context.Query<User>();

            q.Select(a => Sql.Count()).First();
            /*
             * SELECT COUNT(1) AS `C` FROM `Users` AS `Users` LIMIT 0,1
             */

            q.Select(a => new { Count = Sql.Count(), LongCount = Sql.LongCount(), Sum = Sql.Sum(a.Age), Max = Sql.Max(a.Age), Min = Sql.Min(a.Age), Average = Sql.Average(a.Age) }).First();
            /*
             * SELECT COUNT(1) AS `Count`,COUNT(1) AS `LongCount`,SUM(`Users`.`Age`) AS `Sum`,MAX(`Users`.`Age`) AS `Max`,MIN(`Users`.`Age`) AS `Min`,AVG(`Users`.`Age`) AS `Average` FROM `Users` AS `Users` LIMIT 0,1
             */

            var count = q.Count();
            /*
             * SELECT COUNT(1) AS `C` FROM `Users` AS `Users`
             */

            var longCount = q.LongCount();
            /*
             * SELECT COUNT(1) AS `C` FROM `Users` AS `Users`
             */

            var sum = q.Sum(a => a.Age);
            /*
             * SELECT SUM(`Users`.`Age`) AS `C` FROM `Users` AS `Users`
             */

            var max = q.Max(a => a.Age);
            /*
             * SELECT MAX(`Users`.`Age`) AS `C` FROM `Users` AS `Users`
             */

            var min = q.Min(a => a.Age);
            /*
             * SELECT MIN(`Users`.`Age`) AS `C` FROM `Users` AS `Users`
             */

            var avg = q.Average(a => a.Age);
            /*
             * SELECT AVG(`Users`.`Age`) AS `C` FROM `Users` AS `Users`
             */

            ConsoleHelper.WriteLineAndReadKey();
        }

        public static void GQTest()
        {
            IQuery<User> q = context.Query<User>();

            IGroupingQuery<User> g = q.Where(a => a.Id > 0).GroupBy(a => a.Age);

            g = g.Having(a => a.Age > 1 && Sql.Count() > 0);

            g.Select(a => new { a.Age, Count = Sql.Count(), Sum = Sql.Sum(a.Age), Max = Sql.Max(a.Age), Min = Sql.Min(a.Age), Avg = Sql.Average(a.Age) }).ToList();
            /*
             * SELECT `Users`.`Age` AS `Age`,COUNT(1) AS `Count`,SUM(`Users`.`Age`) AS `Sum`,MAX(`Users`.`Age`) AS `Max`,MIN(`Users`.`Age`) AS `Min`,AVG(`Users`.`Age`) AS `Avg` FROM `Users` AS `Users` WHERE `Users`.`Id` > 0 GROUP BY `Users`.`Age` HAVING (`Users`.`Age` > 1 AND COUNT(1) > 0)
             */


            ConsoleHelper.WriteLineAndReadKey();
        }

        public static void Insert()
        {
            //返回主键 Id
            int id = (int)context.Insert<User>(() => new User() { Name = "lu", Age = 18, Gender = Gender.Man, CityId = 1, OpTime = DateTime.Now });
            /*
             * INSERT INTO `Users`(`Name`,`Age`,`Gender`,`CityId`,`OpTime`) VALUES(N'lu',18,1,1,NOW());SELECT @@IDENTITY
             */

            User user = new User();
            user.Name = "lu";
            user.Age = 18;
            user.Gender = Gender.Man;
            user.CityId = 1;
            user.OpTime = DateTime.Now;

            //会自动将自增 Id 设置到 user 的 Id 属性上
            user = context.Insert(user);
            /*
             * String ?P_0 = 'lu';
               Gender ?P_1 = Man;
               Int32 ?P_2 = 18;
               Int32 ?P_3 = 1;
               DateTime ?P_4 = '2016/7/22 21:33:58';
               INSERT INTO `Users`(`Name`,`Gender`,`Age`,`CityId`,`OpTime`) VALUES(?P_0,?P_1,?P_2,?P_3,?P_4);SELECT @@IDENTITY
             */


            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void UpdateTest1()
        {
            context.Update<User>(a => a.Id == 1, a => new User() { Name = a.Name, Age = a.Age + 100, Gender = Gender.Man, OpTime = DateTime.Now });
            /*
             * UPDATE `Users` SET `Name`=`Users`.`Name`,`Age`=(`Users`.`Age` + 100),`Gender`=1,`OpTime`=NOW() WHERE `Users`.`Id` = 1
             */

            //批量更新
            //给所有女性年轻 10 岁
            context.Update<User>(a => a.Gender == Gender.Woman, a => new User() { Age = a.Age - 10, OpTime = DateTime.Now });
            /*
             * UPDATE `Users` SET `Age`=(`Users`.`Age` - 10),`OpTime`=NOW() WHERE `Users`.`Gender` = 2
             */

            User user = new User();
            user.Id = 1;
            user.Name = "lu";
            user.Age = 28;
            user.Gender = Gender.Man;
            user.OpTime = DateTime.Now;

            context.Update(user); //会更新所有映射的字段
            /*
             * String ?P_0 = 'lu';
               Gender ?P_1 = Man;
               Int32 ?P_2 = 28;
               Nullable<Int32> ?P_3 = NULL;
               DateTime ?P_4 = '2016/7/22 21:35:14';
               Int32 ?P_5 = 1;
               UPDATE `Users` SET `Name`=?P_0,`Gender`=?P_1,`Age`=?P_2,`CityId`=?P_3,`OpTime`=?P_4 WHERE `Users`.`Id` = ?P_5
             */


            /*
             * 支持只更新属性值已变的属性
             */

            context.TrackEntity(user);//在上下文中跟踪实体
            user.Name = user.Name + "1";
            context.Update(user);//这时只会更新被修改的字段
            /*
             * String ?P_0 = 'lu1';
               Int32 ?P_1 = 1;
               UPDATE `Users` SET `Name`=?P_0 WHERE `Users`.`Id` = ?P_1
             */


            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void DeleteTest1()
        {
            context.Delete<User>(a => a.Id == 1);
            /*
             * DELETE `Users` FROM `Users` WHERE `Users`.`Id` = 1
             */

            //批量删除
            //删除所有不男不女的用户
            context.Delete<User>(a => a.Gender == null);
            /*
             * DELETE `Users` FROM `Users` WHERE `Users`.`Gender` IS NULL
             */

            User user = new User();
            user.Id = 1;
            context.Delete(user);
            /*
             * Int32 ?P_0 = 1;
               DELETE `Users` FROM `Users` WHERE `Users`.`Id` = ?P_0
             */

            ConsoleHelper.WriteLineAndReadKey(1);
        }

        public static void MethodTest1()
        {
            IQuery<User> q = context.Query<User>();

            var space = new char[] { ' ' };

            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now.AddDays(1);

            var ret = q.Select(a => new
            {
                Id = a.Id,

                String_Length = (int?)a.Name.Length,//LENGTH(`Users`.`Name`)
                Substring = a.Name.Substring(0),//SUBSTRING(`Users`.`Name`,0 + 1,LENGTH(`Users`.`Name`))
                Substring1 = a.Name.Substring(1),//SUBSTRING(`Users`.`Name`,1 + 1,LENGTH(`Users`.`Name`))
                Substring1_2 = a.Name.Substring(1, 2),//SUBSTRING(`Users`.`Name`,1 + 1,2)
                ToLower = a.Name.ToLower(),//LOWER(`Users`.`Name`)
                ToUpper = a.Name.ToUpper(),//UPPER(`Users`.`Name`)
                IsNullOrEmpty = string.IsNullOrEmpty(a.Name),//CASE WHEN (`Users`.`Name` IS NULL OR `Users`.`Name` = N'') THEN 1 ELSE 0 END = 1
                Contains = (bool?)a.Name.Contains("s"),//`Users`.`Name` LIKE CONCAT('%',N's','%')
                Trim = a.Name.Trim(),//TRIM(`Users`.`Name`)
                TrimStart = a.Name.TrimStart(space),//LTRIM(`Users`.`Name`)
                TrimEnd = a.Name.TrimEnd(space),//RTRIM(`Users`.`Name`)
                StartsWith = (bool?)a.Name.StartsWith("s"),//`Users`.`Name` LIKE CONCAT(N's','%')
                EndsWith = (bool?)a.Name.EndsWith("s"),//`Users`.`Name` LIKE CONCAT('%',N's')

                DiffYears = Sql.DiffYears(startTime, endTime),//TIMESTAMPDIFF(YEAR,?P_0,?P_1)
                DiffMonths = Sql.DiffMonths(startTime, endTime),//TIMESTAMPDIFF(MONTH,?P_0,?P_1)
                DiffDays = Sql.DiffDays(startTime, endTime),//TIMESTAMPDIFF(DAY,?P_0,?P_1)
                DiffHours = Sql.DiffHours(startTime, endTime),//TIMESTAMPDIFF(HOUR,?P_0,?P_1)
                DiffMinutes = Sql.DiffMinutes(startTime, endTime),//TIMESTAMPDIFF(MINUTE,?P_0,?P_1)
                DiffSeconds = Sql.DiffSeconds(startTime, endTime),//TIMESTAMPDIFF(SECOND,?P_0,?P_1)
                //DiffMilliseconds = Sql.DiffMilliseconds(startTime, endTime),//MySql 不支持 Millisecond
                //DiffMicroseconds = Sql.DiffMicroseconds(startTime, endTime),//ex

                Now = DateTime.Now,//NOW()
                UtcNow = DateTime.UtcNow,//UTC_TIMESTAMP()
                Today = DateTime.Today,//CURDATE()
                Date = DateTime.Now.Date,//CURDATE()
                Year = DateTime.Now.Year,//YEAR(NOW())
                Month = DateTime.Now.Month,//MONTH(NOW())
                Day = DateTime.Now.Day,//DAY(NOW())
                Hour = DateTime.Now.Hour,//HOUR(NOW())
                Minute = DateTime.Now.Minute,//MINUTE(NOW())
                Second = DateTime.Now.Second,//SECOND(NOW())
                Millisecond = DateTime.Now.Millisecond,//?P_2 AS `Millisecond`
                DayOfWeek = DateTime.Now.DayOfWeek,//(DAYOFWEEK(NOW()) - 1)

                Byte_Parse = byte.Parse("1"),//不支持
                Int_Parse = int.Parse("1"),//CAST(N'1' AS SIGNED)
                Int16_Parse = Int16.Parse("11"),//CAST(N'11' AS SIGNED)
                Long_Parse = long.Parse("2"),//CAST(N'2' AS SIGNED)
                Double_Parse = double.Parse("3.1"),//N'3'
                Float_Parse = float.Parse("4.1"),//N'4'
                //Decimal_Parse = decimal.Parse("5"),//不支持
                Guid_Parse = Guid.Parse("D544BC4C-739E-4CD3-A3D3-7BF803FCE179"),//N'D544BC4C-739E-4CD3-A3D3-7BF803FCE179'

                Bool_Parse = bool.Parse("1"),//CAST(N'1' AS SIGNED)
                DateTime_Parse = DateTime.Parse("2014-01-01"),//CAST(N'2014-1-1' AS DATETIME)
            }).ToList();

            ConsoleHelper.WriteLineAndReadKey();
        }

    }

}
