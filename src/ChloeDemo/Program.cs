using Chloe.Infrastructure;
using Chloe.Infrastructure.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChloeDemo
{
    public class Program
    {
        /* documentation：http://www.52chloe.com/Wiki/Document */
        public static void Main(string[] args)
        {
            /* 添加拦截器，输出 sql 语句极其相应的参数 */
            IDbCommandInterceptor interceptor = new DbCommandInterceptor();
            DbConfiguration.UseInterceptors(interceptor);

            DbConfiguration.UseMappingType(new String_MappingType());

            /* fluent mapping */
            DbConfiguration.UseTypeBuilders(typeof(UserMap));

            SQLiteDemo.Run();
            MsSqlDemo.Run();
            MySqlDemo.Run();
            PostgreSQLDemo.Run();
            OracleDemo.Run();
        }
    }
}
