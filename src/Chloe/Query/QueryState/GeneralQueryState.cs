
namespace Chloe.Query.QueryState
{
    class GeneralQueryState : QueryStateBase, IQueryState
    {
        public GeneralQueryState(QueryModel queryModel)
            : base(queryModel)
        {
        }

        public override QueryModel ToFromQueryModel()
        {
            QueryModel newQueryModel = new QueryModel(this.QueryModel.ScopeParameters, this.QueryModel.ScopeTables, this.QueryModel.IgnoreFilters);
            newQueryModel.FromTable = this.QueryModel.FromTable;
            newQueryModel.ResultModel = this.QueryModel.ResultModel;
            if (!this.QueryModel.IgnoreFilters)
            {
                newQueryModel.Filters.AddRange(this.QueryModel.Filters);
            }

            return newQueryModel;
        }

    }
}
