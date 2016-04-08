using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query;
using System.Linq.Expressions;
using Chloe.Query.QueryExpressions;
using Chloe.Query.DbExpressions;
using Chloe.Query.QueryState;
using Chloe.Query.Mapping;

namespace Chloe.Query
{
    public interface IQueryState
    {
        ResultElement Result { get; }
        IQueryState AppendWhereExpression(WhereExpression exp);
        IQueryState AppendOrderExpression(OrderExpression exp);
        //void IncludeNavigationMember(Expression exp);
        IQueryState UpdateSelectResult(SelectExpression selectExpression);
        MappingData GenerateMappingData();
    }
}
