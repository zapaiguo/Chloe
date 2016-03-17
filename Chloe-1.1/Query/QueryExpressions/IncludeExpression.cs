using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public class IncludeExpression : SingleParameterFnQueryExpression
    {
        public IncludeExpression(QueryExpression prevExpression, Type elementType, Expression exp)
            : base(QueryExpressionType.Include, elementType, prevExpression, exp)
        {
        }
    }
}
