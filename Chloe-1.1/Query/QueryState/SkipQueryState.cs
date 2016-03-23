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
        public SkipQueryState(int count, IQueryState prevQueryState)
            : base(prevQueryState)
        {
            this.Count = count;
        }


        public int Count { get; set; }
        public override ResultElement Result
        {
            get
            {
                IQueryState queryState = this.AsSubQueryState();
                return queryState.Result;
            }
        }

        public override void AppendWhereExpression(WhereExpression whereExp)
        {
            throw new NotSupportedException("SkipQueryState.AppendWhereExpression(WhereExpression whereExp)");
        }
        public override void AppendOrderExpression(OrderExpression orderExp)
        {
            throw new NotSupportedException("SkipQueryState.AppendOrderExpression(OrderExpression orderExp)");
        }
        public override DbSqlQueryExpression CreateSqlQuery(out MappingEntity mappingMember)
        {
            ResultElement prevResult = this._prevResult;
            MappingMembers prevPappingMembers = prevResult.MappingMembers;

            TablePart prevTablePart = prevResult.TablePart;
            prevTablePart.SetTableNameByNumber(0);

            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            sqlQuery.Table = prevTablePart;
            sqlQuery.Where = prevResult.WhereExpression;
            sqlQuery.Orders = prevResult.OrderParts;
            sqlQuery.TakeCount = null;
            sqlQuery.SkipCount = this.Count;
            mappingMember = prevPappingMembers.GetMappingEntity(sqlQuery.Columns);

            return sqlQuery;
        }
    }
}
