using Chloe.Query.QueryState;
using System;
using System.Linq.Expressions;

namespace Chloe.Query.QueryExpressions
{
    class SelectExpression : QueryExpression
    {
        LambdaExpression _expression;
        public SelectExpression(Type elementType, QueryExpression prevExpression, LambdaExpression predicate)
            : base(QueryExpressionType.Select, elementType, prevExpression)
        {
            this._expression = predicate;
        }
        public LambdaExpression Expression
        {
            get { return this._expression; }
        }
        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
