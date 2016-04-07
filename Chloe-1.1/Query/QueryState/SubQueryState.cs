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
    internal abstract class SubQueryState : BaseQueryState, IQueryState
    {
        ResultElement _resultElement;
        protected SubQueryState(ResultElement resultElement)
        {
            this._resultElement = resultElement;
            this.Init();
        }

        void Init()
        {
        }

        public override ResultElement Result
        {
            get
            {
                return this._resultElement;
            }
        }

        public override IQueryState AppendWhereExpression(WhereExpression whereExp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.AppendWhereExpression(whereExp);
        }
        public override IQueryState AppendOrderExpression(OrderExpression orderExp)
        {
            IQueryState state = this.AsSubQueryState();
            return state.AppendOrderExpression(orderExp);
        }

        public override IQueryState UpdateSelectResult(SelectExpression selectExpression)
        {
            IQueryState queryState = this.AsSubQueryState();
            return queryState.UpdateSelectResult(selectExpression);
        }
        public override MappingData GenerateMappingData()
        {
            MappingData data = new MappingData();
            IObjectActivtorCreator mappingMember;

            //------------
            DbSqlQueryExpression sqlQuery = this.CreateSqlQuery(out mappingMember);
            //============

            data.SqlQuery = sqlQuery;
            data.MappingEntity = mappingMember;

            return data;
        }

        public virtual IQueryState AsSubQueryState()
        {
            ResultElement prevResult = this.Result;
            IMappingObjectExpression prevMappingMembers = prevResult.MappingObjectExpression;
            IObjectActivtorCreator mappingMember;
            DbSqlQueryExpression sqlQuery = this.CreateSqlQuery(out mappingMember);
            DbSubQueryExpression subQuery = new DbSubQueryExpression(sqlQuery);

            ResultElement result = new ResultElement();
            DbTableExpression tableExp = new DbTableExpression(subQuery, result.GenerateUniqueTableAlias());
            TablePart tablePart = new TablePart(tableExp);

            result.TablePart = tablePart;

            //得将 subQuery.SqlQuery.Orders 告诉 以下创建的 result


            result.IsFromSubQuery = true;

            IMappingObjectExpression mappingMembers = prevMappingMembers;//生成 MappingMembers，目前可以直接用 prevPappingMembers，还没影响
            result.MappingObjectExpression = mappingMembers;

            //将 orderPart 传递下去
            if (prevResult.OrderParts.Count > 0)
            {
                for (int i = 0; i < prevResult.OrderParts.Count; i++)
                {
                    OrderPart orderPart = prevResult.OrderParts[i];
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
                    result.OrderParts.Add(new OrderPart(columnAccessExpression, orderPart.OrderType));
                }
            }

            GeneralQueryState queryState = new GeneralQueryState(result);
            return queryState;
        }
        public abstract DbSqlQueryExpression CreateSqlQuery(out IObjectActivtorCreator mappingMember);
    }
}
