using Chloe.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryExpressions
{
    public abstract class QueryExpressionVisitor<T>
    {
        public virtual T Visit(QueryExpression exp)
        {
            if (exp == null)
                return default(T);
            switch ((QueryExpressionType)exp.NodeType)
            {
                case QueryExpressionType.Root:
                    return this.VisitRoot((RootQueryExpression)exp);
                case QueryExpressionType.Where:
                    return this.VisitWhere((WhereExpression)exp);
                case QueryExpressionType.Take:
                    return this.VisitTake((TakeExpression)exp);
                case QueryExpressionType.Skip:
                    return this.VisitSkip((SkipExpression)exp);
                case QueryExpressionType.OrderBy:
                case QueryExpressionType.OrderByDesc:
                case QueryExpressionType.ThenBy:
                case QueryExpressionType.ThenByDesc:
                    return this.VisitOrder((OrderExpression)exp);

                case QueryExpressionType.Select:
                    return this.VisitSelect((SelectExpression)exp);

                default:
                    throw new NotSupportedException(string.Format("Unhandled queryExpression type: '{0}'", exp.NodeType));
            }
        }
        protected virtual T VisitRoot(RootQueryExpression exp)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitSelect(SelectExpression exp)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitWhere(WhereExpression exp)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitTake(TakeExpression exp)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitSkip(SkipExpression exp)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitOrder(OrderExpression exp)
        {
            throw new NotImplementedException();
        }
    }
}
