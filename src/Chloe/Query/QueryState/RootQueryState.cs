using Chloe.DbExpressions;
using System;
using Chloe.Descriptors;
using System.Reflection;
using Chloe.InternalExtensions;
using System.Linq.Expressions;
using System.Collections.Generic;
using Chloe.Utility;
using Chloe.Infrastructure;

namespace Chloe.Query.QueryState
{
    internal sealed class RootQueryState : QueryStateBase
    {
        Type _elementType;
        public RootQueryState(Type elementType, string explicitTableName, LockType @lock, ScopeParameterDictionary scopeParameters, KeyDictionary<string> scopeTables)
            : base(CreateQueryModel(elementType, explicitTableName, @lock, scopeParameters, scopeTables))
        {
            this._elementType = elementType;
        }

        public override QueryModel ToFromQueryModel()
        {
            if (this.QueryModel.Condition == null)
            {
                QueryModel result = new QueryModel(this.QueryModel.ScopeParameters, this.QueryModel.ScopeTables);
                result.FromTable = this.QueryModel.FromTable;
                result.ResultModel = this.QueryModel.ResultModel;
                return result;
            }

            return base.ToFromQueryModel();
        }

        static QueryModel CreateQueryModel(Type type, string explicitTableName, LockType lockType, ScopeParameterDictionary scopeParameters, KeyDictionary<string> scopeTables)
        {
            if (type.IsAbstract || type.IsInterface)
                throw new ArgumentException("The type of input can not be abstract class or interface.");

            //TODO init queryModel
            QueryModel queryModel = new QueryModel(scopeParameters, scopeTables);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(type);

            DbTable dbTable = typeDescriptor.Table;
            if (explicitTableName != null)
                dbTable = new DbTable(explicitTableName, dbTable.Schema);
            string alias = queryModel.GenerateUniqueTableAlias(dbTable.Name);

            queryModel.FromTable = CreateRootTable(dbTable, alias, lockType);

            ConstructorInfo constructor = typeDescriptor.GetDefaultConstructor();
            if (constructor == null)
                throw new ArgumentException(string.Format("The type of '{0}' does't define a none parameter constructor.", type.FullName));

            ComplexObjectModel model = new ComplexObjectModel(constructor);
           
            DbTable table = new DbTable(alias);

            foreach (PrimitivePropertyDescriptor item in typeDescriptor.PropertyDescriptors)
            {
                DbColumnAccessExpression columnAccessExpression = new DbColumnAccessExpression(table, item.Column);

                model.AddPrimitiveMember(item.Property, columnAccessExpression);
                if (item.IsPrimaryKey)
                    model.PrimaryKey = columnAccessExpression;
            }

            queryModel.ResultModel = model;

            return queryModel;
        }
        static DbFromTableExpression CreateRootTable(DbTable table, string alias, LockType lockType)
        {
            DbTableExpression tableExp = new DbTableExpression(table);
            DbTableSegment tableSeg = new DbTableSegment(tableExp, alias, lockType);
            var fromTableExp = new DbFromTableExpression(tableSeg);
            return fromTableExp;
        }
    }
}
