using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;

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

        public override IQueryState Accept(IQueryState queryState)
        {
            int count = this.Count > 0 ? this.Count : 0;

            TakeQueryState takeQueryState = null;
            SkipQueryState skipQueryState = null;
            LimitQueryState limitQueryState = null;

            if ((skipQueryState = queryState as SkipQueryState) != null)
            {
                limitQueryState = new LimitQueryState(skipQueryState.Count, count, skipQueryState.Result);
                return limitQueryState;
            }
            else if ((takeQueryState = queryState as TakeQueryState) != null)
            {
                takeQueryState.UpdateCount(count);
                return takeQueryState;
            }
            else if ((limitQueryState = queryState as LimitQueryState) != null)
            {
                limitQueryState.UpdateTakeCount(count);
                return limitQueryState;
            }

            takeQueryState = new TakeQueryState(count, queryState.Result);
            return takeQueryState;
        }
    }
}
