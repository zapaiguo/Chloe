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
                    return this.Visit((RootQueryExpression)exp);
                case QueryExpressionType.Where:
                    return this.Visit((WhereExpression)exp);
                case QueryExpressionType.Take:
                    return this.Visit((TakeExpression)exp);
                case QueryExpressionType.Skip:
                    return this.Visit((SkipExpression)exp);
                case QueryExpressionType.OrderBy:
                case QueryExpressionType.OrderByDesc:
                case QueryExpressionType.ThenBy:
                case QueryExpressionType.ThenByDesc:
                    return this.Visit((OrderExpression)exp);
                case QueryExpressionType.Select:
                    return this.Visit((SelectExpression)exp);
                case QueryExpressionType.Function:
                    return this.Visit((FunctionExpression)exp);

                default:
                    throw new NotSupportedException(string.Format("Unhandled queryExpression type: '{0}'", exp.NodeType));
            }
        }
        public virtual T Visit(RootQueryExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(SelectExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(WhereExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(TakeExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(SkipExpression exp)
        {
            throw new NotImplementedException();
        }

        public virtual T Visit(OrderExpression exp)
        {
            throw new NotImplementedException();
        }
        public virtual T Visit(FunctionExpression exp)
        {
            throw new NotImplementedException();
        }

    }
}
