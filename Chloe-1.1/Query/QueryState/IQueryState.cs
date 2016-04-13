using Chloe.Query.QueryExpressions;
using Chloe.Query.Mapping;

namespace Chloe.Query.QueryState
{
    public interface IQueryState
    {
        ResultElement Result { get; }
        MappingData GenerateMappingData();

        IQueryState Accept(WhereExpression exp);
        IQueryState Accept(OrderExpression exp);
        IQueryState Accept(SelectExpression exp);
        IQueryState Accept(SkipExpression exp);
        IQueryState Accept(TakeExpression exp);
    }
}
