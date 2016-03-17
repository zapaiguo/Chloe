using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query.QueryExpressions;

namespace Chloe.Query.QueryExpressions
{
    public class TakeExpression : QueryExpression
    {
        private int _count;
        public TakeExpression(QueryExpression prevExpression, Type elementType, int count)
            : base(QueryExpressionType.Take, elementType, prevExpression)
        {
            _count = count;
        }

        public int Count
        {
            get { return _count; }
        }
    }
}
