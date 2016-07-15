using Chloe.SqlServer;
using Db;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Chloe;
using System.Data.SqlClient;

namespace ChloePerformanceTest
{
    class QueryPerformanceTest
    {
        static int takeCount = 1;
        static int queryCount = 20000;

        public static void GCMemoryTest()
        {
            /*
             * 内存分配测试通过 分析 --> 性能与诊断 --> 内存使用率 测试
             * 每次运行程序只能调用下面中的一个方法，不能同时调用
             */

            for (int i = 0; i < queryCount; i++)
            {
                ChloeQueryTest(takeCount);
                //ChloeSqlQueryTest(takeCount);
                //DapperQueryTest(takeCount);
                //EFLinqQueryTest(takeCount);
                //EFSqlQueryTest(takeCount);
            }
        }
        public static void SpeedTest()
        {
            long useTime = 0;

            //预热
            ChloeQueryTest(1);
            useTime = SW.Do(() =>
            {
                for (int i = 0; i < queryCount; i++)
                {
                    ChloeQueryTest(takeCount);
                }
            });
            Console.WriteLine("ChloeQueryTest 执行{0}次查询总用时：{1}ms", queryCount, useTime);
            GC.Collect();


            useTime = SW.Do(() =>
            {
                for (int i = 0; i < queryCount; i++)
                {
                    ChloeSqlQueryTest(takeCount);
                }
            });
            Console.WriteLine("ChloeSqlQueryTest 执行{0}次查询总用时：{1}ms", queryCount, useTime);
            GC.Collect();


            //预热
            DapperQueryTest(1);
            useTime = SW.Do(() =>
            {
                for (int i = 0; i < queryCount; i++)
                {
                    DapperQueryTest(takeCount);
                }
            });
            Console.WriteLine("DapperQueryTest 执行{0}次查询总用时：{1}ms", queryCount, useTime);
            GC.Collect();

            //预热
            EFLinqQueryTest(1);
            useTime = SW.Do(() =>
            {
                for (int i = 0; i < queryCount; i++)
                {
                    EFLinqQueryTest(takeCount);
                }
            });
            Console.WriteLine("EFLinqQueryTest 执行{0}次查询总用时：{1}ms", queryCount, useTime);
            GC.Collect();


            //预热
            EFSqlQueryTest(1);
            useTime = SW.Do(() =>
            {
                for (int i = 0; i < queryCount; i++)
                {
                    EFSqlQueryTest(takeCount);
                }
            });
            Console.WriteLine("EFSqlQueryTest 执行{0}次查询总用时：{1}ms", queryCount, useTime);
            GC.Collect();


            Console.WriteLine("GAME OVER");
            Console.ReadKey();
        }


        static void ChloeQueryTest(int takeCount)
        {
            using (MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString))
            {
                int id = 0;
                var list = context.Query<TestEntity>().Where(a => a.Id > id).Take(takeCount).ToList();
            }
        }
        static void ChloeSqlQueryTest(int takeCount)
        {
            using (MsSqlContext context = new MsSqlContext(DbHelper.ConnectionString))
            {
                int id = 0;
                var list = context.SqlQuery<TestEntity>(string.Format("select top {0} * from TestEntity where Id>@Id", takeCount.ToString()), DbParam.Create("@Id", id)).ToList();
            }
        }

        static void DapperQueryTest(int takeCount)
        {
            using (IDbConnection conn = DbHelper.CreateConnection())
            {
                int id = 0;
                var list = conn.Query<TestEntity>(string.Format("select top {0} * from TestEntity where Id>@Id", takeCount.ToString()), new { Id = id }).ToList();
            }
        }
        static void EFLinqQueryTest(int takeCount)
        {
            using (EFContext efContext = new EFContext())
            {
                int id = 0;
                var list = efContext.TestEntity.AsNoTracking().Where(a => a.Id > id).Take(takeCount).ToList();
            }
        }
        static void EFSqlQueryTest(int takeCount)
        {
            using (EFContext efContext = new EFContext())
            {
                int id = 0;
                var list = efContext.Database.SqlQuery<TestEntity>(string.Format("select top {0} * from TestEntity where Id>@Id", takeCount.ToString()), new SqlParameter("@Id", id)).ToList();
            }
        }
    }
}
