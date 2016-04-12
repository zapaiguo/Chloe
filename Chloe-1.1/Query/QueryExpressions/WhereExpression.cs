using Chloe.Query.QueryState;
using System;
using System.Linq.Expressions;

namespace Chloe.Query.QueryExpressions
{
    public class WhereExpression : SingleParameterFnQueryExpression
    {
        public WhereExpression(QueryExpression prevExpression, Type elementType, Expression predicate)
            : base(QueryExpressionType.Where, elementType, prevExpression, predicate)
        {
        }

        public override IQueryState Accept(IQueryState queryState)
        {
            return queryState.AppendWhereExpression(this);
        }
        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
