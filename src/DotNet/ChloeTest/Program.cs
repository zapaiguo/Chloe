using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe;
using Chloe.Query;
using Chloe.SqlServer;
using System.Dynamic;
using System.Diagnostics;
using System.Reflection;
using Chloe.Mapper;
using System.Data;
using Chloe.Core;
using Chloe.Core.Emit;
using System.Collections;
using Chloe.Descriptors;
using Chloe.Entity;
using Database;
using System.Reflection.Emit;
using System.Threading;
using Oracle.ManagedDataAccess.Client;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using Chloe.Infrastructure.Interception;
using AutoMapper;
using Chloe.Infrastructure;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Security.Cryptography;

namespace ChloeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            decimal d = 123456789.12345678901234567M;
            var x = Convert.ToDouble(d);
            //Test(1);
            //Test1(1000000);
            //Test(1);
            IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            DbInterception.Add(interceptor);

            int count = 60;


            //RegisterMappingTypeDemo.RunDemo();

            //FeatureTest_Oracle.Test();
            //FeatureTest.Test();
            FeatureTest_MySql.Test();
            //FeatureTest_SQLite.Test();
            //EFTest.Test();
            //Task.Run(() => { });
            //SqliteTest.Test();
            //Console.WriteLine(list.Count);
        }

        public static void Test(int count)
        {
            var prop = typeof(User).GetProperty("Name");

            List<object> list = new List<object>();

            for (int l = 0; l < 3; l++)
            {

                Console.WriteLine("开始");

                for (int i = 0; i < 10000 * 1; i++)
                {
                    //list.Add(MRMHelper.CreateMRM(prop));
                    //list.Add(new TypeDescriptor(typeof(TestEntity)));
                }

                Console.WriteLine(list.Count);
                GC.Collect();
                //ConsoleHelper.WriteLineAndReadKey();
            }

            //ConsoleHelper.WriteLineAndReadKey(111);
        }
        public static void Test1(int count)
        {
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);
            //context.Query<TestEntity>().ToList();
            GC.Collect();

            TestEntity testEntity = new TestEntity();
            object id = 100;

            int loop = 0;
            while (loop < 5)
            {
                loop++;

                Console.WriteLine("开始");

                DateTime start = DateTime.Now;

                for (int i = 0; i < 100000; i++)
                {
                    var prop = typeof(TestEntity).GetProperty("Id");
                    prop.SetValue(testEntity, i);
                }



                //var list = context.Query<TestEntity>().Take(count).ToList();

                var useTime = (DateTime.Now - start).TotalMilliseconds;

                Console.WriteLine(testEntity.Id);
                Console.WriteLine("{0}: {1}", loop, useTime);
                GC.Collect();
                //ConsoleHelper.WriteLineAndReadKey();
            }


            ConsoleHelper.WriteLineAndReadKey();
        }
        public static void Test2(int count)
        {
            string sql = "INSERT INTO [TestEntity]([F_Byte],[F_Int16],[F_Int32],[F_Int64],[F_Double],[F_Float],[F_Decimal],[F_Bool],[F_DateTime],[F_Guid],[F_String]) VALUES(@P_0,@P_1,@P_2,@P_3,@P_4,@P_5,@P_6,@P_7,@P_8,@P_9,@P_10);";
            sql = "INSERT INTO [TestEntity]([F_Byte],[F_Int16],[F_Int32],[F_Int64],[F_Double],[F_Float],[F_Decimal],[F_Bool],[F_DateTime],[F_Guid],[F_String])";
            //sql = "INSERT INTO [TestEntity]([F_Byte],[F_Int16],[F_Int32])";
            StringBuilder sb = new StringBuilder();
            sb.Append(sql);
            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    sb.AppendLine(" union all");
                string dt = "'2017-09-16 18:33:47'";
                string guid = $"'{Guid.NewGuid().ToString()}'";
                sb.Append($" select {1},{2},{3},{i},{i},{i},{i},{1},{dt},{guid},'{i.ToString()}'");
                //sb.Append($" select {1},{2},{3}");
            }

            string cmdText = sb.ToString();
            MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString);

            //SW.Do(() =>
            //{
            context.Session.ExecuteNonQuery(cmdText);
            //});

            //var r = context.Session.ExecuteNonQuery(cmdText);

            //ConsoleHelper.WriteLineAndReadKey();
        }


        public static void Test1()
        {


            Console.WriteLine("aaa");
            Console.Read();
        }

        public static DateTime ResolveDateTime(string text, DateTime currentTime)
        {
            DateTime date = MatchDate(text, currentTime).Date;

            int? hour = MatchTime(text);
            hour = ConvertTimeSystem(text, hour);

            if (date == currentTime.Date)
            {
                if (hour == null)
                {
                    return currentTime;
                }

                if (hour.Value > currentTime.Hour)
                {
                    return date.AddHours(hour.Value);
                }
                else
                {
                    return currentTime;
                }
            }

            /* 非今天 */

            if (hour != null)
            {
                return date.AddHours(hour.Value);
            }

            return date.AddHours(8);/* 设置从 8 点出发 */
        }

        public static int? ConvertTimeSystem(string text, int? hour)
        {
            if (hour == null)
                return null;

            if (text.Contains("下午") || text.Contains("晚上") || text.Contains("中午"))
            {
                if (hour.Value < 13)
                {
                    return hour.Value + 12;
                }
            }

            return hour;
        }
        public static int? MatchTime(string text)
        {
            /*
             * 10点 --> ([0-9]+)点
             * 12点
             *早上9点
             *下午2点
             *晚上8点
             *12:00 --> ([0-9]+:[0-9]{1,2})点*
             *12:00点
             */

            int? hour = null;

            string pattern = "";
            pattern = "([0-9]+):[0-9]{1,2}点*";


            if (Regex.IsMatch(text, pattern))
            {
                Match match = Regex.Match(text, pattern);
                string matchText = match.Groups[1].Value;
                if (matchText.Length <= 2)
                {
                    hour = int.Parse(matchText);
                    if (hour.Value == 24)
                        hour = 0;
                    if (hour.Value < 24)
                    {
                        return hour;
                    }
                }
            }

            pattern = "([0-9]+)点";

            if (Regex.IsMatch(text, pattern))
            {
                Match match = Regex.Match(text, pattern);
                string matchText = match.Groups[1].Value;
                if (matchText.Length <= 2)
                {
                    hour = int.Parse(matchText);
                    if (hour.Value == 24)
                        hour = 0;
                    if (hour.Value < 24)
                    {
                        return hour;
                    }
                }
            }

            return hour;
        }
        public static DateTime MatchDate(string text, DateTime currentTime)
        {
            if (text.Contains("今天"))
            {
                return currentTime;
            }
            else if (text.Contains("明天"))
            {
                return currentTime.Date.AddDays(1);
            }
            else if (text.Contains("后天"))
            {
                return currentTime.Date.AddDays(2);
            }

            if (text.Contains("周一") || text.Contains("星期一") || text.Contains("礼拜一"))
            {
                return GetClosestDayOfWeekInFuture(currentTime, DayOfWeek.Monday);
            }

            if (text.Contains("周二") || text.Contains("星期二") || text.Contains("礼拜二"))
            {
                return GetClosestDayOfWeekInFuture(currentTime, DayOfWeek.Tuesday);
            }

            if (text.Contains("周三") || text.Contains("星期三") || text.Contains("礼拜三"))
            {
                return GetClosestDayOfWeekInFuture(currentTime, DayOfWeek.Wednesday);
            }

            if (text.Contains("周四") || text.Contains("星期四") || text.Contains("礼拜四"))
            {
                return GetClosestDayOfWeekInFuture(currentTime, DayOfWeek.Thursday);
            }

            if (text.Contains("周五") || text.Contains("星期五") || text.Contains("礼拜五"))
            {
                return GetClosestDayOfWeekInFuture(currentTime, DayOfWeek.Friday);
            }

            if (text.Contains("周六") || text.Contains("星期六") || text.Contains("礼拜六"))
            {
                return GetClosestDayOfWeekInFuture(currentTime, DayOfWeek.Saturday);
            }

            if (text.Contains("周日") || text.Contains("星期日") || text.Contains("礼拜日"))
            {
                return GetClosestDayOfWeekInFuture(currentTime, DayOfWeek.Sunday);
            }

            if (text.Contains("周末"))
            {
                if (currentTime.DayOfWeek == DayOfWeek.Sunday || currentTime.DayOfWeek == DayOfWeek.Saturday)
                    return currentTime;
                else
                    return GetClosestDayOfWeekInFuture(currentTime, DayOfWeek.Saturday);
            }

            return currentTime;
        }
        static DateTime GetClosestDayOfWeekInFuture(DateTime currentTime, DayOfWeek dw)
        {
            if (currentTime.DayOfWeek == dw)
                return currentTime;

            if (currentTime.DayOfWeek > dw)
            {
                return currentTime.AddDays(7 - (currentTime.DayOfWeek - dw));
            }
            else
            {
                return currentTime.AddDays(dw - currentTime.DayOfWeek);
            }
        }
    }

    public class Test : IDisposable
    {
        //[Column(IsPrimaryKey = true)]
        //[AutoIncrementAttribute]
        public Guid Id { get; set; }
        //[AutoIncrementAttribute]
        public string Name { get; set; }

        public void Dispose()
        {
            Console.WriteLine(8888);
        }

        public string T()
        {
            return "12345678";
        }
        public string TT()
        {
            return this.T();
        }

        public static void Fn(int i)
        {

        }
        public static void Fn<T1>(int i)
        {

        }
    }
}
