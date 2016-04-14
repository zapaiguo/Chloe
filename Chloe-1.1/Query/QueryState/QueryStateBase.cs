using Chloe.DbExpressions;
using Chloe.Query.Mapping;
using Chloe.Query.QueryExpressions;
using Chloe.Query.Visitors;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

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
            ResultElement result = this.CreateNewResult(exp);
            return this.CreateQueryState(result);
        }
        public virtual IQueryState Accept(SkipExpression exp)
        {
            SkipQueryState state = new SkipQueryState(this.Result, exp.Count);
            return state;
        }
        public virtual IQueryState Accept(TakeExpression exp)
        {
            TakeQueryState state = new TakeQueryState(this.Result, exp.Count);
            return state;
        }
        public virtual IQueryState Accept(FunctionExpression exp)
        {
            ExpressionVisitorBase visitor = this.Visitor;

            List<DbExpression> dbParameters = new List<DbExpression>(exp.Parameters.Count);
            foreach (Expression pExp in exp.Parameters)
            {
                var dbExp = visitor.Visit(pExp);
                dbParameters.Add(dbExp);
            }

            DbFunctionExpression dbFuncExp = new DbFunctionExpression(exp.ElementType, exp.Method, dbParameters);
            MappingFieldExpression mfe = new MappingFieldExpression(exp.ElementType, dbFuncExp);

            ResultElement result = new ResultElement();

            result.MappingObjectExpression = mfe;
            result.FromTable = this._resultElement.FromTable;
            result.UpdateCondition(this._resultElement.Where);

            FunctionQueryState state = new FunctionQueryState(result);
            return state;
        }

        public virtual ResultElement CreateNewResult(SelectExpression exp)
        {
            ResultElement result = new ResultElement();
            result.FromTable = this._resultElement.FromTable;

            SelectExpressionVisitor visistor = new SelectExpressionVisitor(this.Visitor, this._resultElement.MappingObjectExpression);

            IMappingObjectExpression r = visistor.Visit(exp.Expression);
            result.MappingObjectExpression = r;
            result.OrderSegments.AddRange(this._resultElement.OrderSegments);
            result.UpdateCondition(this._resultElement.Where);

            return result;
        }
        public virtual IQueryState CreateQueryState(ResultElement result)
        {
            return new GeneralQueryState(result);
        }

        public virtual MappingData GenerateMappingData()
        {
            MappingData data = new MappingData();

            DbSqlQueryExpression sqlQuery = this.CreateSqlQuery();

            var moe = this._resultElement.MappingObjectExpression.GenarateObjectActivtorCreator(sqlQuery);

            data.SqlQuery = sqlQuery;
            data.MappingEntity = moe;

            return data;
        }

        public virtual GeneralQueryState AsSubQueryState()
        {
            DbSqlQueryExpression sqlQuery = this.CreateSqlQuery();
            DbSubQueryExpression subQuery = new DbSubQueryExpression(sqlQuery);

            ResultElement result = new ResultElement();

            DbTableExpression tableExp = new DbTableExpression(subQuery, result.GenerateUniqueTableAlias());
            DbFromTableExpression tablePart = new DbFromTableExpression(tableExp);

            result.FromTable = tablePart;

            //TODO 根据旧的生成新 MappingMembers
            IMappingObjectExpression newMoe = this.Result.MappingObjectExpression.ToNewObjectExpression(sqlQuery, tableExp);
            result.MappingObjectExpression = newMoe;

            //得将 subQuery.SqlQuery.Orders 告诉 以下创建的 result
            //将 orderPart 传递下去
            if (this.Result.OrderSegments.Count > 0)
            {
                for (int i = 0; i < this.Result.OrderSegments.Count; i++)
                {
                    DbOrderSegmentExpression orderPart = this.Result.OrderSegments[i];
                    DbExpression orderExp = orderPart.DbExpression;

                    string alias = null;

                    DbColumnExpression columnExpression = sqlQuery.Columns.Where(a => DbExpressionEqualizer.Equals(orderExp, a.Body)).FirstOrDefault();

                    // 对于重复的则不需要往 sqlQuery.Columns 重复添加了
                    if (columnExpression != null)
                    {
                        alias = columnExpression.Alias;
                    }
                    else
                    {
                        alias = sqlQuery.GenerateUniqueColumnAlias();
                        DbColumnExpression columnExp = new DbColumnExpression(orderExp.Type, orderExp, alias);
                        sqlQuery.Columns.Add(columnExp);
                    }

                    DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(orderExp.Type, tableExp, alias);
                    result.OrderSegments.Add(new DbOrderSegmentExpression(columnAccessExpression, orderPart.OrderType));
                }
            }

            result.IsFromSubQuery = true;

            GeneralQueryState queryState = new GeneralQueryState(result);
            return queryState;
        }
        public virtual DbSqlQueryExpression CreateSqlQuery()
        {
            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            var tablePart = this._resultElement.FromTable;

            sqlQuery.Table = tablePart;
            sqlQuery.Orders.AddRange(this._resultElement.OrderSegments);
            sqlQuery.UpdateWhereExpression(this._resultElement.Where);

            return sqlQuery;
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
