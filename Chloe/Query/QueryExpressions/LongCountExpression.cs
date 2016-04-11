using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public class LongCountExpression : QueryExpression
    {
        public LongCountExpression(QueryExpression prevExpression)
            : base(QueryExpressionType.LongCount, prevExpression)
        {
        }
    }
}
