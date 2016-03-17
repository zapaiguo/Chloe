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
        int _skipCount;
        int _takeCount;
        public LimitQueryState(int skipCount, int takeCount, IQueryState prevQueryState)
            : base(prevQueryState)
        {
            this._skipCount = skipCount;
            this._takeCount = takeCount;
        }

        public int SkipCount
        {
            get { return _skipCount; }
            set { _skipCount = value; }
        }
        public int TakeCount
        {
            get { return _takeCount; }
            set { _takeCount = value; }
        }
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
            throw new NotSupportedException("LimitQueryState.AppendWhereExpression(WhereExpression whereExp)");
        }
        public override void AppendOrderExpression(OrderExpression orderExp)
        {
            throw new NotSupportedException("LimitQueryState.AppendOrderExpression(OrderExpression orderExp)");
        }

        public override DbSqlQueryExpression CreateSqlQuery(MappingMember mappingMember)
        {
            ResultElement prevResult = this._prevResult;
            MappingMembers prevPappingMembers = prevResult.MappingMembers;

            TablePart prevTablePart = prevResult.TablePart;
            prevTablePart.SetTableNameByNumber(0);

            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            sqlQuery.Table = prevTablePart;
            sqlQuery.Where = prevResult.WhereExpression;
            sqlQuery.Orders = prevResult.OrderParts;
            sqlQuery.TakeCount = this._takeCount;
            sqlQuery.SkipCount = this._skipCount; ;
            FillColumnList(sqlQuery.Columns, prevPappingMembers, mappingMember);

            return sqlQuery;
        }
    }
}
