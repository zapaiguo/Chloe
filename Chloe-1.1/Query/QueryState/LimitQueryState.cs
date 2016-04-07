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
        //int _skipCount;
        //int _takeCount;
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

        public override DbSqlQueryExpression CreateSqlQuery(out IObjectActivtorCreator mappingMember)
        {
            ResultElement prevResult = this.Result;
            var prevPappingMembers = prevResult.MappingObjectExpression;

            TablePart prevTablePart = prevResult.TablePart;
            //prevTablePart.SetTableNameByNumber(0);

            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            sqlQuery.Table = prevTablePart;
            sqlQuery.Where = prevResult.WhereExpression;
            sqlQuery.Orders.AddRange(prevResult.OrderParts);
            sqlQuery.TakeCount = this.TakeCount;
            sqlQuery.SkipCount = this.SkipCount;
            mappingMember = prevPappingMembers.GenarateObjectActivtorCreator(sqlQuery);

            return sqlQuery;
        }
    }
}
