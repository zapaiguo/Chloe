
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
                QueryModel result = new QueryModel(this.QueryModel.ScopeParameters, this.QueryModel.ScopeTables);
                result.FromTable = this.QueryModel.FromTable;
                result.ResultModel = this.QueryModel.ResultModel;
                return result;
            }

            return base.ToFromQueryModel();
        }

    }
}
