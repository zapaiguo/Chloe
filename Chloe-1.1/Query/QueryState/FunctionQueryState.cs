using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryState
{
    class FunctionQueryState : QueryStateBase, IQueryState
    {
        public FunctionQueryState(ResultElement resultElement)
            : base(resultElement)
        {
        }
    }
}
