using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Visitors
{
    class JoinQueryExpressionVisitor : QueryExpressionVisitor<JoinQueryResult>
    {
        ResultElement _resultElement;
        JoinType _joinType;

        LambdaExpression _conditionExpression;
        List<IMappingObjectExpression> _moeList;

        JoinQueryExpressionVisitor(ResultElement resultElement, JoinType joinType, LambdaExpression conditionExpression, List<IMappingObjectExpression> moeList)
        {
            this._resultElement = resultElement;
            this._joinType = joinType;
            this._conditionExpression = conditionExpression;
            this._moeList = moeList;
        }

        public static JoinQueryResult VisitQueryExpression(QueryExpression queryExpression, ResultElement resultElement, JoinType joinType, LambdaExpression conditionExpression, List<IMappingObjectExpression> moeList)
        {
            JoinQueryExpressionVisitor visitor = new JoinQueryExpressionVisitor(resultElement, joinType, conditionExpression, moeList);
            return queryExpression.Accept(visitor);
        }

        public override JoinQueryResult Visit(RootQueryExpression exp)
        {
            Type type = exp.ElementType;
            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(type);

            DbTableSegmentExpression tableExp = CreateTableExpression(typeDescriptor.TableName, this._resultElement.GenerateUniqueTableAlias(typeDescriptor.TableName));
            MappingObjectExpression moe = new MappingObjectExpression(typeDescriptor.EntityType.GetConstructor(UtilConstants.EmptyTypeArray));

            foreach (MappingMemberDescriptor item in typeDescriptor.MappingMemberDescriptors)
            {
                DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(tableExp, item.Column);
                moe.AddMemberExpression(item.MemberInfo, columnAccessExpression);

                if (item.IsPrimaryKey)
                    moe.PrimaryKey = columnAccessExpression;
            }

            //TODO 解析 on 条件表达式
            DbExpression condition = null;
            List<IMappingObjectExpression> moeList = new List<IMappingObjectExpression>(this._moeList.Count + 1);
            moeList.AddRange(this._moeList);
            moeList.Add(moe);
            condition = GeneralExpressionVisitor.VisitPredicate(this._conditionExpression, moeList);

            DbJoinTableExpression joinTable = new DbJoinTableExpression(this._joinType, tableExp, this._resultElement.FromTable, condition);

            JoinQueryResult result = new JoinQueryResult();
            result.MappingObjectExpression = moe;
            result.JoinTable = joinTable;

            return result;
        }
        public override JoinQueryResult Visit(WhereExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult Visit(OrderExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult Visit(SelectExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult Visit(SkipExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult Visit(TakeExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult Visit(FunctionExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult Visit(JoinQueryExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }
        public override JoinQueryResult Visit(GroupingQueryExpression exp)
        {
            JoinQueryResult ret = this.Visit(exp);
            return ret;
        }

        JoinQueryResult Visit(QueryExpression exp)
        {
            IQueryState state = QueryExpressionVisitor.VisitQueryExpression(exp);
            JoinQueryResult ret = state.ToJoinQueryResult(this._joinType, this._conditionExpression, this._resultElement.FromTable, this._moeList, this._resultElement.GenerateUniqueTableAlias());
            return ret;
        }
        static DbTableSegmentExpression CreateTableExpression(string tableName, string alias)
        {
            DbTableExpression rootTable = new DbTableExpression(tableName);
            DbTableSegmentExpression tableExp = new DbTableSegmentExpression(rootTable, alias);
            return tableExp;
        }
    }
}
