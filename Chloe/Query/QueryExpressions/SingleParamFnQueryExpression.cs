using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public class SingleParamFnQueryExpression : QueryExpression
    {
        private Expression _expression;
        public SingleParamFnQueryExpression(QueryExpressionType nodeType, QueryExpression prevExpression, Expression expression)
            : base(nodeType, prevExpression)
        {
            this._expression = expression;
        }

        public Expression Expression
        {
            get { return _expression; }
        }

    }
}
