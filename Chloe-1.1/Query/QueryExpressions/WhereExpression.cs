using Chloe.Query.QueryState;
using System;
using System.Linq.Expressions;

namespace Chloe.Query.QueryExpressions
{
    public class WhereExpression : QueryExpression
    {
        public WhereExpression(QueryExpression prevExpression, Type elementType, Expression predicate)
            : base(QueryExpressionType.Where, elementType, prevExpression)
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
