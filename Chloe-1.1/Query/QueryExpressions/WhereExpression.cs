using Chloe.Query.QueryState;
using System;
using System.Linq.Expressions;

namespace Chloe.Query.QueryExpressions
{
    public class WhereExpression : QueryExpression
    {
        Expression _expression;
        public WhereExpression(QueryExpression prevExpression, Type elementType, Expression predicate)
            : base(QueryExpressionType.Where, elementType, prevExpression)
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
