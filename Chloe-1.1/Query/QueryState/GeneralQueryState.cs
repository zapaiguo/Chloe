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
    class GeneralQueryState : BaseQueryState, IQueryState
    {
        //IQueryState _prevQueryState;
        ResultElement _result;
        SelectEntity _rawEntity;
        public GeneralQueryState(/*IQueryState prevQueryState,*/ ResultElement result)
        {
            //this._prevQueryState = prevQueryState;
            this._result = result;
            this.Init();
        }

        void Init()
        {
            SelectEntity rawEntity = new SelectEntity(this._result);
            this._rawEntity = rawEntity;
        }

        public override ResultElement Result
        {
            get
            {
                return this.GetResultElement();
            }
        }

        /// <summary>
        /// ps:该方法有 bug ，当多次调用 GetResultElement() 会得到意想不到的结果
        /// </summary>
        /// <returns></returns>
        ResultElement GetResultElement()
        {
            BaseExpressionVisitor visitor = new GeneralExpressionVisitor(this._rawEntity);

            //在这解析所有表达式树，如 where、order、select、IncludeNavigationMember 等
            //解析 where 表达式，得出的 DbExpression 

            this._result.UpdateWhereExpression(this.VisistWhereExpressions(visitor));

            //在这判断，如果当前的 state 未指定任何 Order ，则使用上个句子的
            if (this._result.IsFromSubQuery)
            {
                if (this.OrderExpressions.Count > 0)
                {
                    this._result.OrderParts.Clear();
                    this.VisistOrderExpressions(visitor, this._result.OrderParts);
                    this._result.IsFromSubQuery = false;
                }
            }
            else if (this.OrderExpressions.Count > 0)
            {
                this.VisistOrderExpressions(visitor, this._result.OrderParts);
                this._result.IsFromSubQuery = false;
            }

            return this._result;
        }

        public override MappingData GenerateMappingData()
        {
            MappingData data = new MappingData();
            //MappingEntity mappingMember = new MappingEntity(this._result.MappingMembers.Constructor);

            //------------
            DbSqlQueryExpression sqlQuery = new DbSqlQueryExpression();
            BaseExpressionVisitor visitor = new GeneralExpressionVisitor(this._rawEntity);
            TablePart tablePart = this._result.TablePart;

            DbExpression whereDbExpression = this.VisistWhereExpressions(visitor);
            sqlQuery.UpdateWhereExpression(this._result.WhereExpression);
            sqlQuery.UpdateWhereExpression(whereDbExpression);

            //在这判断，如果当前的 state 未指定任何 Order ，则使用上个句子的
            if (this._result.IsFromSubQuery == false)
            {
                sqlQuery.Orders.AddRange(this._result.OrderParts);
                this.VisistOrderExpressions(visitor, sqlQuery.Orders);
            }
            else if (this._result.IsFromSubQuery && this.OrderExpressions.Count == 0)
            {
                sqlQuery.Orders.AddRange(this._result.OrderParts);
            }
            else
                this.VisistOrderExpressions(visitor, sqlQuery.Orders);

            tablePart.SetTableNameByNumber(0);
            MappingEntity mappingMember = this._result.MappingMembers.GetMappingEntity(sqlQuery.Columns);
            //FillColumnList(sqlQuery.Columns, this._result.MappingMembers, mappingMember);
            sqlQuery.Table = tablePart;
            //============

            data.SqlQuery = sqlQuery;
            data.MappingEntity = mappingMember;

            return data;
        }

        public override IQueryState UpdateSelectResult(SelectExpression selectExpression)
        {
            ResultElement result = new ResultElement(this._result.TablePart);

            //解析 where order 表达式树
            //解析 selectExpression
            //构建一个新的 ResultElement
            BaseExpressionVisitor visitor = new GeneralExpressionVisitor(this._rawEntity);
            SelectExpressionVisitor selectExpressionVisitor = new GeneralSelectExpressionVisitor(visitor, this._result);
            MappingMembers mappingMembers = selectExpressionVisitor.Visit(selectExpression.Expression);

            result.MappingMembers = mappingMembers;

            result.UpdateWhereExpression(this._result.WhereExpression);
            result.UpdateWhereExpression(VisistWhereExpressions(visitor, this.WhereExpressions));

            //在这判断，如果当前的 state 未指定任何 Order ，则使用上个句子的
            if (this._result.IsFromSubQuery == false)
            {
                result.OrderParts.AddRange(this._result.OrderParts);
                this.VisistOrderExpressions(visitor, result.OrderParts);
            }
            else if (this._result.IsFromSubQuery && this.OrderExpressions.Count == 0)
            {
                result.OrderParts.AddRange(this._result.OrderParts);
                result.IsFromSubQuery = true;
            }
            else
                this.VisistOrderExpressions(visitor, result.OrderParts);

            return new GeneralQueryState(result);
        }
    }
}
