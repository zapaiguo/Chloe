using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query;
using Chloe.Query.QueryExpressions;

namespace Chloe.Core
{
    public interface IQuery //: IEnumerable
    {
        QueryExpression QueryExpression { get; }
    }
}
