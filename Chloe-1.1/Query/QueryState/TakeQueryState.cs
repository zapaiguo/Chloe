using Chloe.DbExpressions;

namespace Chloe.Query.QueryState
{
    internal sealed class TakeQueryState : SubQueryState
    {
        public TakeQueryState(int count, ResultElement resultElement)
            : base(resultElement)
        {
            this.Count = count;
        }

        public int Count { get; private set; }
        public void UpdateCount(int count)
        {
            if (count < this.Count)
            {
                this.Count = count;
            }
        }

        public override DbSqlQueryExpression CreateSqlQuery()
        {
            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            sqlQuery.Table = this.Result.FromTable;
            sqlQuery.Where = this.Result.Where;
            sqlQuery.Orders.AddRange(this.Result.OrderSegments);
            sqlQuery.TakeCount = this.Count;
            sqlQuery.SkipCount = null;

            return sqlQuery;
        }
    }
}
