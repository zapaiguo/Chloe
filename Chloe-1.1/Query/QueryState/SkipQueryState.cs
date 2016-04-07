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
    internal sealed class SkipQueryState : SubQueryState
    {
        public SkipQueryState(int count, ResultElement resultElement)
            : base(resultElement)
        {
            this.Count = count;
        }


        public int Count { get; set; }

        public override DbSqlQueryExpression CreateSqlQuery(out IObjectActivtorCreator mappingMember)
        {
            ResultElement prevResult = this.Result;
            var prevPappingMembers = prevResult.MappingObjectExpression;

            TablePart prevTablePart = prevResult.TablePart;
            prevTablePart.SetTableNameByNumber(0);

            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            sqlQuery.Table = prevTablePart;
            sqlQuery.Where = prevResult.WhereExpression;
            sqlQuery.Orders.AddRange(prevResult.OrderParts);
            sqlQuery.TakeCount = null;
            sqlQuery.SkipCount = this.Count;
            mappingMember = prevPappingMembers.GenarateObjectActivtorCreator(sqlQuery);

            return sqlQuery;
        }
    }
}
