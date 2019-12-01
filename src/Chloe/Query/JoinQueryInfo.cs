using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Chloe.Query
{
    class JoinQueryInfo
    {
        public JoinQueryInfo(QueryBase query, JoinType joinType, LambdaExpression condition)
        {
            this.Query = query;
            this.JoinType = joinType;
            this.Condition = condition;
        }
        public QueryBase Query { get; set; }
        public JoinType JoinType { get; set; }
        public LambdaExpression Condition { get; set; }
    }
}
