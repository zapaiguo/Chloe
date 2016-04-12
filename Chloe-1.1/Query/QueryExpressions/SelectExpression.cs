using Chloe.Query.QueryState;
using System;
using System.Linq.Expressions;

namespace Chloe.Query.QueryExpressions
{
    public class SelectExpression : SingleParameterFnQueryExpression
    {
        public SelectExpression(QueryExpression prevExpression, Type elementType, Expression selectExpression)
            : base(QueryExpressionType.Select, elementType, prevExpression, selectExpression)
        {
        }

        public override IQueryState Accept(IQueryState queryState)
        {
            IQueryState state = queryState.UpdateSelectResult(this);
            return state;
        }
        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
