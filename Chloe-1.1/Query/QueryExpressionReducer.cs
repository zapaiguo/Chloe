using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;
using Chloe.Query;
using Chloe.Query.DbExpressions;

namespace Chloe.Query
{
    public class QueryExpressionReducer
    {
        IQueryState _queryState;
        QueryExpressionReducer()
        {
        }

        public static IQueryState ReduceQueryExpression(QueryExpression queryExpression)
        {
            QueryExpressionReducer reducer = new QueryExpressionReducer();
            IQueryState queryState = reducer.Reduce(queryExpression);
            return queryState;
        }

        void AddWhereExpression(WhereExpression exp)
        {
            IQueryState queryState = this._queryState;

            SubQueryState subQueryState = queryState as SubQueryState;
            if (subQueryState != null)
            {
                queryState = subQueryState.AsSubQueryState();
                this._queryState = queryState;
            }

            queryState.AppendWhereExpression(exp);
        }
        void AddOrderExpression(OrderExpression exp)
        {
            IQueryState queryState = this._queryState;

            SubQueryState subQueryState = queryState as SubQueryState;
            if (subQueryState != null)
            {
                queryState = subQueryState.AsSubQueryState();
                this._queryState = queryState;
            }

            queryState.AppendOrderExpression(exp);
        }

        protected virtual IQueryState Reduce(QueryExpression queryExpression)
        {
            List<QueryExpression> queryExpressions = new List<QueryExpression>();
            queryExpressions.Add(queryExpression);
            while (queryExpression.PrevExpression != null)
            {
                queryExpression = queryExpression.PrevExpression;
                queryExpressions.Add(queryExpression);
            }

            int maxIndex = queryExpressions.Count - 1;
            for (int i = maxIndex; i >= 0; i--)
            {
                this.Visit(queryExpressions[i]);
            }

            return this._queryState;
        }

        protected virtual void Visit(QueryExpression exp)
        {
            if (exp == null)
                return;
            switch ((QueryExpressionType)exp.NodeType)
            {
                case QueryExpressionType.Root:
                    this.VisitRoot((RootQueryExpression)exp);
                    break;
                case QueryExpressionType.Where:
                    this.VisitWhere((WhereExpression)exp);
                    break;
                case QueryExpressionType.Take:
                    this.VisitTake((TakeExpression)exp);
                    break;
                case QueryExpressionType.Skip:
                    this.VisitSkip((SkipExpression)exp);
                    break;
                case QueryExpressionType.OrderBy:
                case QueryExpressionType.OrderByDesc:
                case QueryExpressionType.ThenBy:
                case QueryExpressionType.ThenByDesc:
                    this.VisitOrder((OrderExpression)exp);
                    break;

                case QueryExpressionType.Select:
                    this.VisitSelect((SelectExpression)exp);
                    break;
                case QueryExpressionType.Include:
                    this.VisitInclude((IncludeExpression)exp);
                    break;

                default:
                    throw new Exception(string.Format("Unhandled queryExpression type: '{0}'", exp.NodeType));
            }
        }
        protected virtual void VisitRoot(RootQueryExpression exp)
        {
            this._queryState = new RootQueryState(exp.ElementType);
            return;
        }

        protected virtual void VisitSelect(SelectExpression exp)
        {
            IQueryState state = this._queryState.UpdateSelectResult(exp);
            this._queryState = state;
            return;
        }

        protected virtual void VisitInclude(IncludeExpression exp)
        {
            this._queryState.IncludeNavigationMember(exp.Expression);
            return;
        }

        protected virtual void VisitWhere(WhereExpression exp)
        {
            this.AddWhereExpression(exp);
            return;
        }

        protected virtual void VisitTake(TakeExpression exp)
        {
            int count = exp.Count > 0 ? exp.Count : 0;

            TakeQueryState takeQuery = null;
            SkipQueryState skipQuery = null;
            LimitQueryState limitQuery = null;

            if ((skipQuery = this._queryState as SkipQueryState) != null)
            {
                limitQuery = new LimitQueryState(skipQuery.Count, count, skipQuery.PrevQueryState);
                _queryState = limitQuery;
                return;
            }
            else if ((takeQuery = this._queryState as TakeQueryState) != null)
            {
                if (count < takeQuery.Count)
                {
                    takeQuery.Count = count;
                }
                return;
            }
            else if ((limitQuery = this._queryState as LimitQueryState) != null)
            {
                if (count < limitQuery.TakeCount)
                {
                    limitQuery.TakeCount = count;
                }
                return;
            }

            takeQuery = new TakeQueryState(count, this._queryState);
            _queryState = takeQuery;
            return;
        }

        protected virtual void VisitSkip(SkipExpression exp)
        {
            if (exp.Count < 1)
            {
                return;
            }

            SkipQueryState skipQuery = null;
            if ((skipQuery = this._queryState as SkipQueryState) != null)
            {
                skipQuery.Count += exp.Count;
                return;
            }

            skipQuery = new SkipQueryState(exp.Count, this._queryState);
            this._queryState = skipQuery;
            return;
        }

        protected virtual void VisitOrder(OrderExpression exp)
        {
            this.AddOrderExpression(exp);
            return;
        }
    }

}
