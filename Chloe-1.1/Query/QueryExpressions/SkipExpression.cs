using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query.QueryExpressions;

namespace Chloe.Query.QueryExpressions
{
    public class SkipExpression : QueryExpression
    {
        private int _count;
        public SkipExpression(QueryExpression prevExpression, Type elementType, int count)
            : base(QueryExpressionType.Skip, elementType, prevExpression)
        {
            _count = count;
        }

        public int Count
        {
            get { return _count; }
        }
    }
}
