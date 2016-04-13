using Chloe.DbExpressions;
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
        protected QueryStateBase(ResultElement resultElement)
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

        public virtual IQueryState Accept(WhereExpression exp)
        {
            ExpressionVisitorBase visitor = this.Visitor;
            var dbExp = visitor.Visit(exp.Expression);
            this._resultElement.UpdateCondition(dbExp);

            return this;
        }
        public virtual IQueryState Accept(OrderExpression exp)
        {
            if (exp.NodeType == QueryExpressionType.OrderBy || exp.NodeType == QueryExpressionType.OrderByDesc)
                this._resultElement.OrderSegments.Clear();

            ExpressionVisitorBase visitor = this.Visitor;
            var r = VisistOrderExpression(visitor, exp);

            if (this._resultElement.IsFromSubQuery)
            {
                this._resultElement.OrderSegments.Clear();
                this._resultElement.IsFromSubQuery = false;
            }

            this._resultElement.OrderSegments.Add(r);

            return this;
        }
        public virtual IQueryState Accept(SelectExpression exp)
        {
            ResultElement result = new ResultElement();
            result.FromTable = this._resultElement.FromTable;

            SelectExpressionVisitor visistor = new SelectExpressionVisitor(this.Visitor, this._resultElement.MappingObjectExpression);

            IMappingObjectExpression r = visistor.Visit(exp.Expression);
            result.MappingObjectExpression = r;
            result.OrderSegments.AddRange(this._resultElement.OrderSegments);
            result.UpdateCondition(this._resultElement.Where);

            return new GeneralQueryState(result);
        }
        public virtual IQueryState Accept(SkipExpression exp)
        {
            SkipQueryState state = new SkipQueryState(exp.Count, this.Result);
            return state;
        }
        public virtual IQueryState Accept(TakeExpression exp)
        {
            TakeQueryState state = new TakeQueryState(exp.Count, this.Result);
            return state;
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
