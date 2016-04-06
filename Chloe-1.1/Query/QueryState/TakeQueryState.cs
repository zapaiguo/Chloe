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
    internal sealed class TakeQueryState : SubQueryState
    {
        public TakeQueryState(int count, IQueryState prevQueryState)
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
            throw new NotSupportedException("TakeQueryState.AppendWhereExpression(WhereExpression whereExp)");
        }
        public override void AppendOrderExpression(OrderExpression orderExp)
        {
            throw new NotSupportedException("TakeQueryState.AppendOrderExpression(OrderExpression orderExp)");
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
            sqlQuery.Orders.AddRange(prevResult.OrderParts);
            sqlQuery.TakeCount = this.Count;
            sqlQuery.SkipCount = null;
            mappingMember = prevPappingMembers.GetMappingEntity(sqlQuery);
            //FillColumnList(sqlQuery.Columns, prevPappingMembers, mappingMember);

            return sqlQuery;
        }
    }
}
