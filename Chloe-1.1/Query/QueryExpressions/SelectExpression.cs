using Chloe.Query.QueryState;
using System;
using System.Linq.Expressions;

namespace Chloe.Query.QueryExpressions
{
    public class SelectExpression : QueryExpression
    {
        Expression _expression;
        public SelectExpression(Type elementType, QueryExpression prevExpression, Expression predicate)
            : base(QueryExpressionType.Select, elementType, prevExpression)
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
