using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;

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

        public override IQueryState Accept(IQueryState queryState)
        {
            if (this.Count < 1)
            {
                return queryState;
            }

            SkipQueryState skipQueryState = null;
            if ((skipQueryState = queryState as SkipQueryState) != null)
            {
                skipQueryState.Count += this.Count;
            }
            else
                skipQueryState = new SkipQueryState(this.Count, queryState.Result);

            return skipQueryState;
        }
    }
}
