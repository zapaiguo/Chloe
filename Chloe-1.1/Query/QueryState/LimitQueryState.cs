using Chloe.Query.DbExpressions;
using Chloe.Query.Implementation;
using Chloe.Query.Mapping;
using Chloe.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryState
{
    internal sealed class LimitQueryState : SubQueryState
    {
        public LimitQueryState(int skipCount, int takeCount, ResultElement resultElement)
            : base(resultElement)
        {
            this.SkipCount = skipCount;
            this.TakeCount = takeCount;
        }

        public int SkipCount
        {
            get;
            private set;
        }
        public int TakeCount
        {
            get;
            private set;
        }

        public void UpdateTakeCount(int count)
        {
            if (count < this.TakeCount)
            {
                this.TakeCount = count;
            }
        }

        public override DbSqlQueryExpression CreateSqlQuery()
        {
            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            sqlQuery.Table = this.Result.TablePart;
            sqlQuery.Where = this.Result.WhereExpression;
            sqlQuery.Orders.AddRange(this.Result.OrderParts);
            sqlQuery.TakeCount = this.TakeCount;
            sqlQuery.SkipCount = this.SkipCount;

            return sqlQuery;
        }
    }
}
