using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public class MaxExpression : SingleParamFnQueryExpression
    {
        public MaxExpression(QueryExpression prevExpression, Expression selector)
            : base(QueryExpressionType.Max, prevExpression, selector)
        {
        }
    }
}
