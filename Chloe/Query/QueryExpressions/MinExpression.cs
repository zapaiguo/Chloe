using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public class MinExpression : SingleParamFnQueryExpression
    {
        public MinExpression(QueryExpression prevExpression, Expression selector)
            : base(QueryExpressionType.Min, prevExpression, selector)
        {
        }
    }
}
