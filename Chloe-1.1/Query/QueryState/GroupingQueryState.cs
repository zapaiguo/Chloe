using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryState
{
    class GroupingQueryState : QueryStateBase
    {
        //List<DbExpression> _groupSegments;
        //DbExpression _havingCondition;
        public GroupingQueryState(ResultElement resultElement, List<DbExpression> groupSegments, DbExpression havingCondition)
            : base(resultElement)
        {
            //this._groupSegments = groupSegments;
            //this._havingCondition = havingCondition;
        }

        //public override DbSqlQueryExpression CreateSqlQuery()
        //{
        //    DbSqlQueryExpression sqlQuery = base.CreateSqlQuery();

        //    sqlQuery.GroupSegments.AddRange(this._groupSegments);
        //    sqlQuery.HavingCondition = this._havingCondition;

        //    return sqlQuery;
        //}

    }
}
