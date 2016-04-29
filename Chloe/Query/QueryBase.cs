using Chloe.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    abstract class QueryBase
    {
        public abstract QueryExpression QueryExpression { get; }
    }

}
