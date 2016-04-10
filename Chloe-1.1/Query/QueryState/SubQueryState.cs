using Chloe.Query.DbExpressions;
using Chloe.Query.Implementation;
using Chloe.Query.Mapping;
using Chloe.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.QueryState
{
    internal abstract class SubQueryState : IQueryState
    {
        ResultElement _resultElement;
        protected SubQueryState(ResultElement resultElement)
        {
            this._resultElement = resultElement;
        }

        public ResultElement Result
        {
            get
            {
                return this._resultElement;
            }
        }

        public virtual IQueryState AppendWhereExpression(WhereExpression whereExp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.AppendWhereExpression(whereExp);
        }
        public virtual IQueryState AppendOrderExpression(OrderExpression orderExp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.AppendOrderExpression(orderExp);
        }
        public virtual IQueryState UpdateSelectResult(SelectExpression selectExpression)
        {
            IQueryState queryState = this.AsSubQueryState();
            return queryState.UpdateSelectResult(selectExpression);
        }

        public virtual MappingData GenerateMappingData()
        {
            MappingData data = new MappingData();

            DbSqlQueryExpression sqlQuery = this.CreateSqlQuery();
            IObjectActivtorCreator moe = this.Result.MappingObjectExpression.GenarateObjectActivtorCreator(sqlQuery);

            data.SqlQuery = sqlQuery;
            data.MappingEntity = moe;

            return data;
        }

        public virtual IQueryState AsSubQueryState()
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
