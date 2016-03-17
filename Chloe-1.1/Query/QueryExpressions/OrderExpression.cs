using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public class OrderExpression : SingleParameterFnQueryExpression
    {
        public OrderExpression(QueryExpressionType expressionType, Type elementType, QueryExpression prevExpression, Expression predicate)
            : base(expressionType, elementType, prevExpression, predicate)
        {
        }
    }

}
