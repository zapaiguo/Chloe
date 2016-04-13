using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;
using Chloe.Query.Visitors;

namespace Chloe.Query.QueryExpressions
{
    public class TakeExpression : QueryExpression
    {
        private int _count;
        public TakeExpression(Type elementType, QueryExpression prevExpression, int count)
            : base(QueryExpressionType.Take, elementType, prevExpression)
        {
            if (count < 0)
            {
                throw new ArgumentException("count 小于 0");
            }

            _count = count;
        }

        public int Count
        {
            get { return _count; }
        }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
