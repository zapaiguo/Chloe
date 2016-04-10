using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public abstract class SingleParameterFnQueryExpression : QueryExpression
    {
        Expression _expression;
        public SingleParameterFnQueryExpression(QueryExpressionType nodeType, Type elementType, QueryExpression prevExpression, Expression expression)
            : base(nodeType, elementType, prevExpression)
        {
            this._expression = expression;
        }

        public Expression Expression
        {
            get { return _expression; }
        }

    }
}
