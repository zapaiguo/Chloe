using Chloe.Query.QueryExpressions;
using Chloe.Query.Mapping;

namespace Chloe.Query.QueryState
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
