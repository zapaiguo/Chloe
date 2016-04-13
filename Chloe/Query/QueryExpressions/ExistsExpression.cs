using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public class ExistsExpression : QueryExpression
    {
        public ExistsExpression(QueryExpression prevExpression)
            : base(QueryExpressionType.Exists, prevExpression)
        {

        }
    }
}
