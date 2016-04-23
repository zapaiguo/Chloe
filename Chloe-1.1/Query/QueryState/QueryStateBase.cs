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
        List<IMappingObjectExpression> _moeList = null;
        protected QueryStateBase(ResultElement resultElement)
        {
            this._resultElement = resultElement;
        }

        protected List<IMappingObjectExpression> MoeList
        {
            get
            {
                if (this._moeList == null)
                    this._moeList = new List<IMappingObjectExpression>(1) { this._resultElement.MappingObjectExpression };

                return this._moeList;
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
            var dbExp = GeneralExpressionVisitor.VisitPredicate(exp.Expression, this.MoeList);
            this._resultElement.AppendCondition(dbExp);

            return this;
        }
        public virtual IQueryState Accept(OrderExpression exp)
        {
            if (exp.NodeType == QueryExpressionType.OrderBy || exp.NodeType == QueryExpressionType.OrderByDesc)
                this._resultElement.OrderSegments.Clear();

            var r = VisistOrderExpression(this.MoeList, exp);

            if (this._resultElement.IsOrderSegmentsFromSubQuery)
            {
                this._resultElement.OrderSegments.Clear();
                this._resultElement.IsOrderSegmentsFromSubQuery = false;
            }

            this._resultElement.OrderSegments.Add(r);

            return this;
        }
        public virtual IQueryState Accept(SelectExpression exp)
        {
            ResultElement result = this.CreateNewResult(exp.Selector);
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
            List<DbExpression> dbParameters = new List<DbExpression>(exp.Parameters.Count);
            foreach (Expression pExp in exp.Parameters)
            {
                var dbExp = GeneralExpressionVisitor.VisitPredicate((LambdaExpression)pExp, this.MoeList);
                dbParameters.Add(dbExp);
            }

            DbFunctionExpression dbFuncExp = new DbFunctionExpression(exp.ElementType, exp.Method, dbParameters);
            MappingFieldExpression mfe = new MappingFieldExpression(exp.ElementType, dbFuncExp);

            ResultElement result = new ResultElement();

            result.MappingObjectExpression = mfe;
            result.FromTable = this._resultElement.FromTable;
            result.AppendCondition(this._resultElement.Condition);

            FunctionQueryState state = new FunctionQueryState(result);
            return state;
        }
        public virtual IQueryState Accept(GroupingQueryExpression exp)
        {
            List<IMappingObjectExpression> moeList = this.MoeList;
            foreach (var item in exp.GroupPredicates)
            {
                var dbExp = GeneralExpressionVisitor.VisitPredicate(item, moeList);
                this._resultElement.GroupSegments.Add(dbExp);
            }

            foreach (var item in exp.HavingPredicates)
            {
                var dbExp = GeneralExpressionVisitor.VisitPredicate(item, moeList);
                this._resultElement.AppendHavingCondition(dbExp);
            }

            var newResult = this.CreateNewResult(exp.Selector);
            return new GroupingQueryState(newResult);
        }

        public virtual ResultElement CreateNewResult(LambdaExpression selector)
        {
            ResultElement result = new ResultElement();
            result.FromTable = this._resultElement.FromTable;

            IMappingObjectExpression r = SelectExpressionVisitor.VisitSelectExpression(selector, this.MoeList);
            result.MappingObjectExpression = r;
            result.OrderSegments.AddRange(this._resultElement.OrderSegments);
            result.AppendCondition(this._resultElement.Condition);

            result.GroupSegments.AddRange(this._resultElement.GroupSegments);
            result.AppendHavingCondition(this._resultElement.HavingCondition);

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

            result.IsOrderSegmentsFromSubQuery = true;

            GeneralQueryState queryState = new GeneralQueryState(result);
            return queryState;
        }
        public virtual DbSqlQueryExpression CreateSqlQuery()
        {
            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();

            sqlQuery.Table = this._resultElement.FromTable;
            sqlQuery.OrderSegments.AddRange(this._resultElement.OrderSegments);
            sqlQuery.Condition = this._resultElement.Condition;

            sqlQuery.GroupSegments.AddRange(this._resultElement.GroupSegments);
            sqlQuery.HavingCondition = this._resultElement.HavingCondition;

            return sqlQuery;
        }

        protected static DbOrderSegmentExpression VisistOrderExpression(List<IMappingObjectExpression> moeList, OrderExpression orderExp)
        {
            DbExpression dbExpression = GeneralExpressionVisitor.VisitPredicate(orderExp.Expression, moeList);
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

        public virtual FromQueryResult ToFromQueryResult()
        {
            DbSqlQueryExpression sqlQuery = this.CreateSqlQuery();
            DbSubQueryExpression subQuery = new DbSubQueryExpression(sqlQuery);

            DbTableExpression tableExp = new DbTableExpression(subQuery, ResultElement.DefaultTableAlias);
            DbFromTableExpression tablePart = new DbFromTableExpression(tableExp);

            IMappingObjectExpression newMoe = this.Result.MappingObjectExpression.ToNewObjectExpression(sqlQuery, tableExp);

            FromQueryResult result = new FromQueryResult();
            result.FromTable = tablePart;
            result.MappingObjectExpression = newMoe;
            return result;
        }

        public virtual JoinQueryResult ToJoinQueryResult(JoinType joinType, LambdaExpression conditionExpression, DbFromTableExpression fromTable, List<IMappingObjectExpression> moeList, string tableAlias)
        {
            DbSqlQueryExpression sqlQuery = this.CreateSqlQuery();
            DbSubQueryExpression subQuery = new DbSubQueryExpression(sqlQuery);

            string alias = tableAlias;
            DbTableExpression tableExp = new DbTableExpression(subQuery, alias);

            IMappingObjectExpression newMoe = this.Result.MappingObjectExpression.ToNewObjectExpression(sqlQuery, tableExp);

            List<IMappingObjectExpression> moes = new List<IMappingObjectExpression>(moeList.Count + 1);
            moes.AddRange(moeList);
            moes.Add(newMoe);
            DbExpression condition = GeneralExpressionVisitor.VisitPredicate(conditionExpression, moes);

            DbJoinTableExpression joinTable = new DbJoinTableExpression(joinType, tableExp, fromTable, condition);

            JoinQueryResult result = new JoinQueryResult();
            result.MappingObjectExpression = newMoe;
            result.JoinTable = joinTable;
            return result;
        }
    }
}
