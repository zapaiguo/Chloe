using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Query.QueryExpressions;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Visitors
{
    class JoinQueryExpressionVisitor : QueryExpressionVisitor<JoinQueryResult>
    {
        JoinType _joinType;
        DbTableExpression _table;
        DbTableExpression _relatedTable;
        DbFromTableExpression _fromTable;
        ResultElement _resultElement;
        JoinQueryExpressionVisitor(JoinType joinType, DbTableExpression table, DbTableExpression relatedTable)
        {

        }

        public override JoinQueryResult Visit(RootQueryExpression exp)
        {
            Type type = exp.ElementType;
            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(type);

            DbTableExpression tableExp = CreateTableExpression(typeDescriptor.TableName, this._resultElement.GenerateUniqueTableAlias(typeDescriptor.TableName));
            MappingObjectExpression moe = new MappingObjectExpression(typeDescriptor.EntityType.GetConstructor(UtilConstants.EmptyTypeArray));

            foreach (MappingMemberDescriptor item in typeDescriptor.MappingMemberDescriptors)
            {
                DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(item.MemberType, tableExp, item.ColumnName);
                moe.AddMemberExpression(item.MemberInfo, columnAccessExpression);
            }

            //TODO 解析 on 条件表达式
            //var visitor = new GeneralExpressionVisitor(moe);
            //visitor

            DbExpression condition = null;

            DbJoinTableExpression joinTable = new DbJoinTableExpression(this._joinType, tableExp, this._relatedTable, this._fromTable, condition);

            JoinQueryResult result = new JoinQueryResult();
            result.MappingObjectExpression = moe;
            result.JoinTable = joinTable;

            return result;
        }

        static DbTableExpression CreateTableExpression(string tableName, string alias)
        {
            DbDerivedTableExpression rootTable = new DbDerivedTableExpression(tableName);
            DbTableExpression tableExp = new DbTableExpression(rootTable, alias);
            return tableExp;
        }
    }

    class JoinQueryResult
    {
        public IMappingObjectExpression MappingObjectExpression { get; set; }
        public DbJoinTableExpression JoinTable { get; set; }
    }
}
