using Chloe.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Chloe
{
#if netcore
    public static partial class DbContextExtension
    {
        /// <summary>
        /// int id = 1;
        /// dbContext.FormatSqlQuery&lt;User&gt;($"select Id,Name from Users where Id={id}");
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static List<T> FormatSqlQuery<T>(this IDbContext dbContext, FormattableString sql)
        {
            /*
             * Usage:
             * int id = 1;
             * dbContext.FormatSqlQuery<User>($"select Id,Name from Users where Id={id}");
             */

            (string Sql, DbParam[] Parameters) r = BuildSqlAndParameters(dbContext, sql);
            return dbContext.SqlQuery<T>(r.Sql, r.Parameters);
        }
        public static List<T> FormatSqlQuery<T>(this IDbContext dbContext, FormattableString sql, CommandType cmdType)
        {
            /*
             * Usage:
             * int id = 1;
             * dbContext.FormatSqlQuery<User>($"select Id,Name from Users where Id={id}");
             */

            (string Sql, DbParam[] Parameters) r = BuildSqlAndParameters(dbContext, sql);
            return dbContext.SqlQuery<T>(r.Sql, cmdType, r.Parameters);
        }
        public static async Task<List<T>> FormatSqlQueryAsync<T>(this IDbContext dbContext, FormattableString sql)
        {
            (string Sql, DbParam[] Parameters) r = BuildSqlAndParameters(dbContext, sql);
            return await dbContext.SqlQueryAsync<T>(r.Sql, r.Parameters);
        }
        public static async Task<List<T>> FormatSqlQueryAsync<T>(this IDbContext dbContext, FormattableString sql, CommandType cmdType)
        {
            (string Sql, DbParam[] Parameters) r = BuildSqlAndParameters(dbContext, sql);
            return await dbContext.SqlQueryAsync<T>(r.Sql, cmdType, r.Parameters);
        }

        static (string Sql, DbParam[] Parameters) BuildSqlAndParameters(IDbContext dbContext, FormattableString sql)
        {
            List<string> formatArgs = new List<string>(sql.ArgumentCount);
            List<DbParam> parameters = new List<DbParam>(sql.ArgumentCount);

            IDatabaseProvider databaseProvider = ((DbContext)dbContext).DatabaseProvider;
            string parameterPrefix = "P_";

            foreach (var arg in sql.GetArguments())
            {
                object paramValue = arg;
                if (paramValue == null || paramValue == DBNull.Value)
                {
                    formatArgs.Add("NULL");
                    continue;
                }

                Type paramType = arg.GetType();

                if (paramType.IsEnum)
                {
                    paramType = Enum.GetUnderlyingType(paramType);
                    if (paramValue != null)
                        paramValue = Convert.ChangeType(paramValue, paramType);
                }

                DbParam p;
                p = parameters.Where(a => PublicHelper.AreEqual(a.Value, paramValue)).FirstOrDefault();

                if (p != null)
                {
                    formatArgs.Add(p.Name);
                    continue;
                }

                string paramName = databaseProvider.CreateParameterName(parameterPrefix + parameters.Count.ToString());
                p = DbParam.Create(paramName, paramValue, paramType);
                parameters.Add(p);
                formatArgs.Add(p.Name);
            }

            string runSql = string.Format(sql.Format, formatArgs.ToArray());
            return (runSql, parameters.ToArray());
        }
    }
#endif
}
