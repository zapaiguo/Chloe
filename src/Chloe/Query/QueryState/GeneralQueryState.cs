
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
            if (this.QueryModel.Condition == null)
            {
                QueryModel newQueryModel = new QueryModel(this.QueryModel.ScopeParameters, this.QueryModel.ScopeTables, this.QueryModel.IgnoreFilters);
                newQueryModel.FromTable = this.QueryModel.FromTable;
                newQueryModel.ResultModel = this.QueryModel.ResultModel;
                return newQueryModel;
            }

            return base.ToFromQueryModel();
        }

    }
}
