using Chloe.Query.QueryState;
using System;
using System.Linq.Expressions;

namespace Chloe.Query.QueryExpressions
{
    public class OrderExpression : QueryExpression
    {
        public OrderExpression(QueryExpressionType expressionType, Type elementType, QueryExpression prevExpression, Expression predicate)
            : base(expressionType, elementType, prevExpression)
        {
            this.Expression = predicate;
        }
        public Expression Expression
        {
            get;
            private set;
        }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}
