using Chloe.DbExpressions;
using System;
using Chloe.Utility;
using Chloe.Descriptors;

namespace Chloe.Query.QueryState
{
    internal sealed class RootQueryState : QueryStateBase
    {
        Type _elementType;
        public RootQueryState(Type elementType)
            : base(CreateResultElement(elementType))
        {
            this._elementType = elementType;
        }

        public override FromQueryResult ToFromQueryResult()
        {
            if (this.Result.Condition == null)
            {
                FromQueryResult result = new FromQueryResult();
                result.FromTable = this.Result.FromTable;
                result.MappingObjectExpression = this.Result.MappingObjectExpression;
                return result;
            }

            return base.ToFromQueryResult();
        }

        static ResultElement CreateResultElement(Type type)
        {
            //TODO init _resultElement
            ResultElement resultElement = new ResultElement();

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(type);

            resultElement.FromTable = CreateRootTable(typeDescriptor.TableName, resultElement.GenerateUniqueTableAlias(typeDescriptor.TableName));
            MappingObjectExpression moe = new MappingObjectExpression(typeDescriptor.EntityType.GetConstructor(UtilConstants.EmptyTypeArray));

            DbTableSegmentExpression tableExp = resultElement.FromTable.Table;
            DbTable table = new DbTable(tableExp.Alias);
            foreach (MappingMemberDescriptor item in typeDescriptor.MappingMemberDescriptors)
            {
                DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(table, item.Column);

                moe.AddMemberExpression(item.MemberInfo, columnAccessExpression);
                if (item.IsPrimaryKey)
                    moe.PrimaryKey = columnAccessExpression;
            }

            resultElement.MappingObjectExpression = moe;

            return resultElement;
        }
        static DbFromTableExpression CreateRootTable(string tableName, string alias)
        {
            DbTable rootTable = new DbTable(tableName);
            DbTableExpression tableExp = new DbTableExpression(rootTable);
            DbTableSegmentExpression tableSegExp = new DbTableSegmentExpression(tableExp, alias);
            var table = new DbFromTableExpression(tableSegExp);
            return table;
        }
    }
}
