using Chloe.Descriptors;
using Chloe.Exceptions;
using Chloe.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe
{
    public static partial class DbContextExtension
    {
        /// <summary>
        /// int id = 1;
        /// context.FormatSqlQuery&lt;User&gt;($"select Id,Name from Users where id={id}");
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static IEnumerable<T> FormatSqlQuery<T>(this IDbContext dbContext, FormattableString sql)
        {
            /*
             * Usage:
             * int id = 1;
             * context.FormatSqlQuery<User>($"select Id,Name from Users where id={id}").ToList();
             */

            List<string> formatArgs = new List<string>(sql.ArgumentCount);
            List<DbParam> parameters = new List<DbParam>(sql.ArgumentCount);

            string parameterPrefix = GetParameterPrefix(dbContext) + "P_";

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
                p = parameters.Where(a => Utils.AreEqual(a.Value, paramValue)).FirstOrDefault();

                if (p != null)
                {
                    formatArgs.Add(p.Name);
                    continue;
                }

                string paramName = parameterPrefix + parameters.Count.ToString();
                p = DbParam.Create(paramName, paramValue, paramType);
                parameters.Add(p);
                formatArgs.Add(p.Name);
            }

            string runSql = string.Format(sql.Format, formatArgs.ToArray());
            return dbContext.SqlQuery<T>(runSql, parameters.ToArray());
        }

        static string GetParameterPrefix(IDbContext dbContext)
        {
            Type dbContextType = dbContext.GetType();
            while (true)
            {
                if (dbContextType == null)
                    break;

                string dbContextTypeName = dbContextType.Name;
                switch (dbContextTypeName)
                {
                    case "MsSqlContext":
                    case "SQLiteContext":
                        return "@";
                    case "MySqlContext":
                        return "?";
                    case "OracleContext":
                        return ":";
                    default:
                        dbContextType = dbContextType.BaseType;
                        break;
                }
            }

            throw new NotSupportedException(dbContext.GetType().FullName);
        }
    }
}
