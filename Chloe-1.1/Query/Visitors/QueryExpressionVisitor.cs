using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;

namespace Chloe.Query.Visitors
{
    public class QueryExpressionVisitor : QueryExpressionVisitor<IQueryState>
    {
        static QueryExpressionVisitor _reducer = new QueryExpressionVisitor();
        QueryExpressionVisitor()
        {
        }
        public static IQueryState VisitQueryExpression(QueryExpression queryExpression)
        {
            return queryExpression.Accept(_reducer);
        }

        protected override IQueryState Visit(RootQueryExpression exp)
        {
            var queryState = new RootQueryState(exp.ElementType);
            return queryState;
        }
        protected override IQueryState Visit(WhereExpression exp)
        {
            var prevState = this.Visit(exp.PrevExpression);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
        protected override IQueryState Visit(SelectExpression exp)
        {
            var prevState = this.Visit(exp.PrevExpression);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
        protected override IQueryState Visit(OrderExpression exp)
        {
            var prevState = this.Visit(exp.PrevExpression);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
        protected override IQueryState Visit(TakeExpression exp)
        {
            var prevState = this.Visit(exp.PrevExpression);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        protected override IQueryState Visit(SkipExpression exp)
        {
            var prevState = this.Visit(exp.PrevExpression);
            IQueryState state = prevState.Accept(exp);
            return state;
        }

        protected override IQueryState Visit(FunctionExpression exp)
        {
            var prevState = this.Visit(exp.PrevExpression);
            IQueryState state = prevState.Accept(exp);
            return state;
        }
    }
}
