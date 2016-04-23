using Chloe.DbExpressions;
using Chloe.Query.QueryExpressions;
using System;
using System.Linq.Expressions;

namespace Chloe.Query.QueryState
{
    internal sealed class LimitQueryState : SubQueryState
    {
        int _skipCount;
        int _takeCount;
        public LimitQueryState(ResultElement resultElement, int skipCount, int takeCount)
            : base(resultElement)
        {
            this.SkipCount = skipCount;
            this.TakeCount = takeCount;
        }

        public int SkipCount
        {
            get
            {
                return this._skipCount;
            }
            set
            {
                this.CheckInputCount(value, "skipCount");
                this._skipCount = value;
            }
        }
        public int TakeCount
        {
            get
            {
                return this._takeCount;
            }
            set
            {
                this.CheckInputCount(value, "takeCount");
                this._takeCount = value;
            }
        }
        void CheckInputCount(int count, string parameName)
        {
            if (count < 0)
            {
                throw new ArgumentException(parameName + " 小于 0");
            }
        }

        public override IQueryState Accept(SelectExpression exp)
        {
            ResultElement result = this.CreateNewResult(exp.Expression);
            return this.CreateQueryState(result);
        }

        public override IQueryState Accept(TakeExpression exp)
        {
            if (exp.Count < this.TakeCount)
                this.TakeCount = exp.Count;

            return this;
        }
        public override IQueryState CreateQueryState(ResultElement result)
        {
            return new LimitQueryState(result, this.SkipCount, this.TakeCount);
        }

        public override DbSqlQueryExpression CreateSqlQuery()
        {
            DbSqlQueryExpression sqlQuery = base.CreateSqlQuery();

            sqlQuery.TakeCount = this.TakeCount;
            sqlQuery.SkipCount = this.SkipCount;

            return sqlQuery;
        }
    }
}
