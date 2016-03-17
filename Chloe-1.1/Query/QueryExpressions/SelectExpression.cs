using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public class SelectExpression : SingleParameterFnQueryExpression
    {
        public SelectExpression(QueryExpression prevExpression, Type elementType, Expression selectExpression)
            : base(QueryExpressionType.Select, elementType, prevExpression, selectExpression)
        {
        }
    }
}
