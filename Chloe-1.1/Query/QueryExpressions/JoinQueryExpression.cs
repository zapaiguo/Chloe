using Chloe.Query.QueryState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    class JoinQueryExpression : QueryExpression
    {
        QueryBase _rootQuery;
        List<JoinedQueryInfo> _joinedQueries;
        LambdaExpression _selector;
        public JoinQueryExpression(QueryBase rootQuery, List<JoinedQueryInfo> joinedQueries, LambdaExpression selector)
            : base(QueryExpressionType.JoinQuery, null, null)
        {
            this._rootQuery = rootQuery;
            this._joinedQueries = new List<JoinedQueryInfo>(joinedQueries.Count);
            this._joinedQueries.AddRange(joinedQueries);
            this._selector = selector;
        }

        public QueryBase RootQuery { get { return this._rootQuery; } }
        public List<JoinedQueryInfo> JoinedQueries { get { return this._joinedQueries; } }
        public LambdaExpression Selector { get { return this._selector; } }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
