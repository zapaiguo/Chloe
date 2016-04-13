using Chloe.Query.QueryState;
using System;
using System.Linq.Expressions;

namespace Chloe.Query.QueryExpressions
{
    public class OrderExpression : QueryExpression
    {
        Expression _expression;
        public OrderExpression(QueryExpressionType expressionType, Type elementType, QueryExpression prevExpression, Expression predicate)
            : base(expressionType, elementType, prevExpression)
        {
            this._expression = predicate;
        }
        public Expression Expression
        {
            get { return this._expression; }
        }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}
