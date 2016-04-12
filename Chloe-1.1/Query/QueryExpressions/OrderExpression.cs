using Chloe.Query.QueryState;
using System;
using System.Linq.Expressions;

namespace Chloe.Query.QueryExpressions
{
    public class OrderExpression : SingleParameterFnQueryExpression
    {
        public OrderExpression(QueryExpressionType expressionType, Type elementType, QueryExpression prevExpression, Expression predicate)
            : base(expressionType, elementType, prevExpression, predicate)
        {
        }

        public override IQueryState Accept(IQueryState queryState)
        {
            IQueryState state = queryState.AppendOrderExpression(this);
            return state;
        }
        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}
