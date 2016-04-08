using Chloe.Query.DbExpressions;
using Chloe.Query.Implementation;
using Chloe.Query.Mapping;
using Chloe.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryState
{
    abstract class BaseQueryState : IQueryState
    {
        ResultElement _resultElement;
        BaseExpressionVisitor _visitor = null;
        public BaseQueryState(ResultElement resultElement)
        {
            this._resultElement = resultElement;
        }

        protected BaseExpressionVisitor Visitor
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
            BaseExpressionVisitor visitor = this.Visitor;
            var dbExp = visitor.Visit(whereExp.Expression);
            this._resultElement.UpdateWhereExpression(dbExp);

            return this;
        }
        public virtual IQueryState AppendOrderExpression(OrderExpression orderExp)
        {
            if (orderExp.NodeType == QueryExpressionType.OrderBy || orderExp.NodeType == QueryExpressionType.OrderByDesc)
                this._resultElement.OrderParts.Clear();

            BaseExpressionVisitor visitor = this.Visitor;
            var r = VisistOrderExpression(visitor, orderExp);

            if (this._resultElement.IsFromSubQuery)
            {
                this._resultElement.OrderParts.Clear();
                this._resultElement.IsFromSubQuery = false;
            }

            this._resultElement.OrderParts.Add(r);

            return this;
        }
        public virtual IQueryState UpdateSelectResult(SelectExpression selectExpression)
        {
            ResultElement result = new ResultElement();
            result.TablePart = this._resultElement.TablePart;

            SelectExpressionVisitor visistor = new SelectExpressionVisitor(this.Visitor, this._resultElement.MappingObjectExpression);

            IMappingObjectExpression r = visistor.Visit(selectExpression.Expression);
            result.MappingObjectExpression = r;
            result.OrderParts.AddRange(this._resultElement.OrderParts);
            result.UpdateWhereExpression(this._resultElement.WhereExpression);

            return new GeneralQueryState(result);
        }

        public virtual MappingData GenerateMappingData()
        {
            MappingData data = new MappingData();

            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            TablePart tablePart = this._resultElement.TablePart;

            sqlQuery.Table = tablePart;
            sqlQuery.Orders.AddRange(this._resultElement.OrderParts);
            sqlQuery.UpdateWhereExpression(this._resultElement.WhereExpression);

            var moe = this._resultElement.MappingObjectExpression.GenarateObjectActivtorCreator(sqlQuery);

            data.SqlQuery = sqlQuery;
            data.MappingEntity = moe;

            return data;
        }

        protected static OrderPart VisistOrderExpression(BaseExpressionVisitor visitor, OrderExpression orderExp)
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

            OrderPart orderPart = new OrderPart(dbExpression, orderType);

            return orderPart;
        }
    }
}
