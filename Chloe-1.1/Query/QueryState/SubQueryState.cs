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
        protected IQueryState _prevQueryState;
        protected ResultElement _prevResult;
        protected SubQueryState(IQueryState prevQueryState)
        {
            this._prevQueryState = prevQueryState;
            this.Init();
        }

        void Init()
        {
            this._prevResult = this._prevQueryState.Result;
        }

        public IQueryState PrevQueryState { get { return this._prevQueryState; } }

        public override IQueryState UpdateSelectResult(SelectExpression selectExpression)
        {
            IQueryState queryState = this.AsSubQueryState();
            return queryState.UpdateSelectResult(selectExpression);
        }
        public override MappingData GenerateMappingData()
        {
            MappingData data = new MappingData(this._prevResult.Type);
            MappingMember mappingMember = new MappingMember(data.EntityType);

            //------------
            DbSqlQueryExpression sqlQuery = this.CreateSqlQuery(mappingMember);
            //============

            data.SqlQuery = sqlQuery;
            data.MappingInfo = mappingMember;

            return data;
        }

        public virtual IQueryState AsSubQueryState()
        {
            ResultElement prevResult = this._prevResult;
            MappingMembers prevPappingMembers = prevResult.MappingMembers;
            DbSqlQueryExpression sqlQuery = this.CreateSqlQuery(null);
            DbSubQueryExpression subQuery = new DbSubQueryExpression(sqlQuery);

            DbTableExpression tableExp = new DbTableExpression(subQuery);

            TablePart tablePart = new TablePart(tableExp);

            //得将 subQuery.SqlQuery.Orders 告诉 以下创建的 result

            ResultElement result = new ResultElement(prevResult.Type, tablePart);
            result.IsFromSubQuery = true;

            MappingMembers mappingMembers = prevPappingMembers;//生成 MappingMembers，目前可以直接用 prevPappingMembers，还没影响
            result.MappingMembers = mappingMembers;

            //将 orderPart 传递下去
            if (prevResult.OrderParts.Count > 0)
            {
                const string c = "C";
                int startIndex = 0;
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
                        alias = c + (startIndex++).ToString();
                        while (sqlQuery.Columns.Any(a => a.Alias == alias))
                        {
                            alias = c + (startIndex++).ToString();
                        }

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
        public abstract DbSqlQueryExpression CreateSqlQuery(MappingMember mappingMember);
    }
}
