using Chloe.DbExpressions;
using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;
using Chloe.Utility;
using System.Collections.Generic;

namespace Chloe.Query.Visitors
{
    class QueryExpressionVisitor : QueryExpressionVisitor<IQueryState>
    {
        static QueryExpressionVisitor _reducer = new QueryExpressionVisitor();
        QueryExpressionVisitor()
        {
        }
        public static IQueryState VisitQueryExpression(QueryExpression queryExpression)
        {
            return queryExpression.Accept(_reducer);
        }

        public override IQueryState Visit(RootQueryExpression exp)
        {
            var queryState = new RootQueryState(exp.ElementType);
            return queryState;
        }
        public override IQueryState Visit(WhereExpression exp)
        {
            var prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
        public override IQueryState Visit(OrderExpression exp)
        {
            var prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
        public override IQueryState Visit(SelectExpression exp)
        {
            var prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
        public override IQueryState Visit(SkipExpression exp)
        {
            var prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
        public override IQueryState Visit(TakeExpression exp)
        {
            var prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
        public override IQueryState Visit(FunctionExpression exp)
        {
            var prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
        public override IQueryState Visit(JoinQueryExpression exp)
        {
            ResultElement resultElement = new ResultElement();

            IQueryState qs = QueryExpressionVisitor.VisitQueryExpression(exp.PrevExpression);
            FromQueryResult fromQueryResult = qs.ToFromQueryResult();

            DbFromTableExpression fromTable = fromQueryResult.FromTable;
            resultElement.FromTable = fromTable;

            List<IMappingObjectExpression> moeList = new List<IMappingObjectExpression>();
            moeList.Add(fromQueryResult.MappingObjectExpression);

            foreach (JoiningQueryInfo joiningQueryInfo in exp.JoinedQueries)
            {
                JoinQueryResult joinQueryResult = JoinQueryExpressionVisitor.VisitQueryExpression(joiningQueryInfo.Query.QueryExpression, resultElement, joiningQueryInfo.JoinType, joiningQueryInfo.Condition, moeList);

                var nullChecking = DbExpression.CaseWhen(new DbCaseWhenExpression.WhenThenExpressionPair(joinQueryResult.JoinTable.Condition, DbConstantExpression.One), DbConstantExpression.Null, DbConstantExpression.One.Type);

                if (joiningQueryInfo.JoinType == JoinType.LeftJoin)
                {
                    joinQueryResult.MappingObjectExpression.SetNullChecking(nullChecking);
                }
                else if (joiningQueryInfo.JoinType == JoinType.RightJoin)
                {
                    foreach (var item in moeList)
                    {
                        item.SetNullChecking(nullChecking);
                    }
                }
                else if (joiningQueryInfo.JoinType == JoinType.FullJoin)
                {
                    joinQueryResult.MappingObjectExpression.SetNullChecking(nullChecking);
                    foreach (var item in moeList)
                    {
                        item.SetNullChecking(nullChecking);
                    }
                }

                fromTable.JoinTables.Add(joinQueryResult.JoinTable);
                moeList.Add(joinQueryResult.MappingObjectExpression);
            }

            IMappingObjectExpression moe = SelectorExpressionVisitor.VisitSelectExpression(exp.Selector, moeList);
            resultElement.MappingObjectExpression = moe;

            GeneralQueryState queryState = new GeneralQueryState(resultElement);
            return queryState;
        }
        public override IQueryState Visit(GroupingQueryExpression exp)
        {
            var prevState = exp.PrevExpression.Accept(this);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
    }
}
