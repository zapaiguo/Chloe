
namespace Chloe.Query.QueryExpressions
{
    public enum QueryExpressionType
    {
        Root = 1,
        Where,
        Take,
        Skip,
        OrderBy,
        OrderByDesc,
        ThenBy,
        ThenByDesc,
        Select,
        Include,
        Function,
    }
}
