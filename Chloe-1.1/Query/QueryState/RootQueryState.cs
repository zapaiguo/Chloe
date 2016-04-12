using Chloe.Query.DbExpressions;
using Chloe.Query.Implementation;
using Chloe.Query.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Chloe.Extensions;
using Chloe.Query.Mapping;
using Chloe.Utility;

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

        static ResultElement CreateResultElement(Type type)
        {
            //TODO init _resultElement
            ResultElement resultElement = new ResultElement();

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(type);

            resultElement.FromTable = CreateRootTable(typeDescriptor.TableName, resultElement.GenerateUniqueTableAlias(typeDescriptor.TableName));
            MappingObjectExpression moe = new MappingObjectExpression(typeDescriptor.EntityType.GetConstructor(UtilConstants.EmptyTypeArray));

            DbTableExpression tableExp = resultElement.FromTable.Table;
            foreach (MappingMemberDescriptor item in typeDescriptor.MappingMemberDescriptors)
            {
                DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(item.MemberType, tableExp, item.ColumnName);
                moe.AddMemberExpression(item.MemberInfo, columnAccessExpression);
            }

            resultElement.MappingObjectExpression = moe;

            return resultElement;
        }
        static DbFromTableExpression CreateRootTable(string tableName, string alias)
        {
            DbDerivedTableExpression rootTable = new DbDerivedTableExpression(tableName);
            DbTableExpression tableExp = new DbTableExpression(rootTable, alias);
            var table = new DbFromTableExpression(tableExp);
            return table;
        }
    }
}
