using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.DbProvider;
using Chloe.Query.DbExpressions;

namespace Chloe.MySql
{
    internal class SqlProvider : IDbProvider
    {
        private SqlProvider()
        {
        }

        public static readonly SqlProvider Instance = new SqlProvider();

        public ISqlBuilder GetSqlBuilder()
        {
            return SqlBuilder.Instance;
        }

        public string TranslateDbExpression(DbExpression dbExpression)
        {
            if (dbExpression == null)
                return null;
            StringBuilder sb = new StringBuilder();
            dbExpression.Accept(SqlExpressionVisitor.Instance).ToString(sb);
            return sb.ToString();
        }
    }
}
