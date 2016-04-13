using Chloe.DbExpressions;
using Chloe.Query.Mapping;
using Chloe.Query.QueryExpressions;
using System;
using System.Linq;

namespace Chloe.Query.QueryState
{
    internal abstract class SubQueryState : QueryStateBase
    {
        protected SubQueryState(ResultElement resultElement)
            : base(resultElement)
        {
        }

        public override IQueryState Accept(WhereExpression exp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.Accept(exp);
        }
        public override IQueryState Accept(OrderExpression exp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.Accept(exp);
        }
        public override IQueryState Accept(SelectExpression exp)
        {
            IQueryState queryState = this.AsSubQueryState();
            return queryState.Accept(exp);
        }
        public override IQueryState Accept(SkipExpression exp)
        {
            IQueryState subQueryState = this.AsSubQueryState();

            SkipQueryState state = new SkipQueryState(exp.Count, subQueryState.Result);
            return state;
        }
        public override IQueryState Accept(TakeExpression exp)
        {
            IQueryState subQueryState = this.AsSubQueryState();

            TakeQueryState state = new TakeQueryState(exp.Count, subQueryState.Result);
            return state;
        }
        public override IQueryState Accept(FunctionExpression exp)
        {
            IQueryState subQueryState = this.AsSubQueryState();

            IQueryState state = subQueryState.Accept(exp);
            return state;
        }

        public override MappingData GenerateMappingData()
        {
            MappingData data = new MappingData();

            DbSqlQueryExpression sqlQuery = this.CreateSqlQuery();
            IObjectActivtorCreator moe = this.Result.MappingObjectExpression.GenarateObjectActivtorCreator(sqlQuery);

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
        public abstract DbSqlQueryExpression CreateSqlQuery();
    }
}
