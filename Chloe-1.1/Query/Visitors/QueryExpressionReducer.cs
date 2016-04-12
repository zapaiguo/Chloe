using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;

namespace Chloe.Query.Visitors
{
    public class QueryExpressionReducer : QueryExpressionVisitor<IQueryState>
    {
        static QueryExpressionReducer _reducer = new QueryExpressionReducer();
        QueryExpressionReducer()
        {
        }
        public static IQueryState ReduceQueryExpression(QueryExpression queryExpression)
        {
            return queryExpression.Accept(_reducer);
        }

        protected override IQueryState VisitRoot(RootQueryExpression exp)
        {
            var queryState = new RootQueryState(exp.ElementType);
            return queryState;
        }

        protected override IQueryState VisitSelect(SelectExpression exp)
        {
            var prevState = this.Visit(exp.PrevExpression);
            IQueryState state = exp.Accept(prevState);
            return state;
        }

        protected override IQueryState VisitWhere(WhereExpression exp)
        {
            var prevState = this.Visit(exp.PrevExpression);
            IQueryState state = exp.Accept(prevState);
            return state;
        }

        protected override IQueryState VisitTake(TakeExpression exp)
        {
            var prevState = this.Visit(exp.PrevExpression);
            IQueryState state = exp.Accept(prevState);
            return state;
        }

        protected override IQueryState VisitSkip(SkipExpression exp)
        {
            var prevState = this.Visit(exp.PrevExpression);
            IQueryState state = exp.Accept(prevState);
            return state;
        }

        protected override IQueryState VisitOrder(OrderExpression exp)
        {
            var prevState = this.Visit(exp.PrevExpression);
            IQueryState state = exp.Accept(prevState);
            return state;
        }
    }

    //public class QueryExpressionReducer1
    //{
    //    QueryExpressionReducer1()
    //    {
    //    }

    //    public static IQueryState ReduceQueryExpression(QueryExpression queryExpression)
    //    {
    //        List<QueryExpression> queryExpressions = new List<QueryExpression>();
    //        queryExpressions.Add(queryExpression);
    //        while (queryExpression.PrevExpression != null)
    //        {
    //            queryExpression = queryExpression.PrevExpression;
    //            queryExpressions.Add(queryExpression);
    //        }

    //        IQueryState queryState = null;
    //        int maxIndex = queryExpressions.Count - 1;
    //        for (int i = maxIndex; i >= 0; i--)
    //        {
    //            queryState = queryExpressions[i].Accept(queryState);
    //        }

    //        return queryState;
    //    }
    //}

}
