using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public class WhereExpression : SingleParameterFnQueryExpression
    {
        public WhereExpression(QueryExpression prevExpression, Type elementType, Expression predicate)
            : base(QueryExpressionType.Where, elementType, prevExpression, predicate)
        {
        }
    }
}
