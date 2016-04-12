using Chloe.DbExpressions;
using Chloe.Query.Implementation;
using Chloe.Query.Mapping;
using Chloe.Query.QueryExpressions;
using Chloe.Query.Visitors;
using System;

namespace Chloe.Query.QueryState
{
    abstract class QueryStateBase : IQueryState
    {
        ResultElement _resultElement;
        ExpressionVisitorBase _visitor = null;
        public QueryStateBase(ResultElement resultElement)
        {
            this._resultElement = resultElement;
        }

        protected ExpressionVisitorBase Visitor
        {
            get
            {
                if (this._visitor == null)
                    _visitor = new GeneralExpressionVisitor(this._resultElement.MappingObjectExpression);

                return this._visitor;
            }
        }
        public virtual ResultElement Result
        {
            get
            {
                return this._resultElement;
            }
        }
        public virtual IQueryState AppendWhereExpression(WhereExpression whereExp)
        {
            ExpressionVisitorBase visitor = this.Visitor;
            var dbExp = visitor.Visit(whereExp.Expression);
            this._resultElement.UpdateWhereExpression(dbExp);

            return this;
        }
        public virtual IQueryState AppendOrderExpression(OrderExpression orderExp)
        {
            if (orderExp.NodeType == QueryExpressionType.OrderBy || orderExp.NodeType == QueryExpressionType.OrderByDesc)
                this._resultElement.OrderSegments.Clear();

            ExpressionVisitorBase visitor = this.Visitor;
            var r = VisistOrderExpression(visitor, orderExp);

            if (this._resultElement.IsFromSubQuery)
            {
                this._resultElement.OrderSegments.Clear();
                this._resultElement.IsFromSubQuery = false;
            }

            this._resultElement.OrderSegments.Add(r);

            return this;
        }
        public virtual IQueryState UpdateSelectResult(SelectExpression selectExpression)
        {
            ResultElement result = new ResultElement();
            result.FromTable = this._resultElement.FromTable;

            //TODO 考虑 q.Select(a => a)、q.Select(a => new {Id=1,A = a})、q.Select(a => a.Id).Take(100).Where(a => a > 0); 等情况，即 SelectExpressionVisitor 还不支持解析 ParameterExpression

            SelectExpressionVisitor visistor = new SelectExpressionVisitor(this.Visitor, this._resultElement.MappingObjectExpression);

            IMappingObjectExpression r = visistor.Visit(selectExpression.Expression);
            result.MappingObjectExpression = r;
            result.OrderSegments.AddRange(this._resultElement.OrderSegments);
            result.UpdateWhereExpression(this._resultElement.Where);

            return new GeneralQueryState(result);
        }

        public virtual MappingData GenerateMappingData()
        {
            MappingData data = new MappingData();

            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            var tablePart = this._resultElement.FromTable;

            sqlQuery.Table = tablePart;
            sqlQuery.Orders.AddRange(this._resultElement.OrderSegments);
            sqlQuery.UpdateWhereExpression(this._resultElement.Where);

            var moe = this._resultElement.MappingObjectExpression.GenarateObjectActivtorCreator(sqlQuery);

            data.SqlQuery = sqlQuery;
            data.MappingEntity = moe;

            return data;
        }

        protected static DbOrderSegmentExpression VisistOrderExpression(ExpressionVisitorBase visitor, OrderExpression orderExp)
        {
            DbExpression dbExpression = visitor.Visit(orderExp.Expression);//解析表达式树 orderExp.Expression
            OrderType orderType;
            if (orderExp.NodeType == QueryExpressionType.OrderBy || orderExp.NodeType == QueryExpressionType.ThenBy)
            {
                orderType = OrderType.Asc;
            }
            else if (orderExp.NodeType == QueryExpressionType.OrderByDesc || orderExp.NodeType == QueryExpressionType.ThenByDesc)
            {
                orderType = OrderType.Desc;
            }
            else
                throw new NotSupportedException(orderExp.NodeType.ToString());

            DbOrderSegmentExpression orderPart = new DbOrderSegmentExpression(dbExpression, orderType);

            return orderPart;
        }
    }
}
