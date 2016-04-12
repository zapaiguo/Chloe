using Chloe.DbExpressions;

namespace Chloe.Query.QueryState
{
    internal sealed class SkipQueryState : SubQueryState
    {
        public SkipQueryState(int count, ResultElement resultElement)
            : base(resultElement)
        {
            this.Count = count;
        }

        public int Count { get; set; }

        public override DbSqlQueryExpression CreateSqlQuery()
        {
            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            sqlQuery.Table = this.Result.FromTable;
            sqlQuery.Where = this.Result.Where;
            sqlQuery.Orders.AddRange(this.Result.OrderSegments);
            sqlQuery.TakeCount = null;
            sqlQuery.SkipCount = this.Count;

            return sqlQuery;
        }
    }
}
