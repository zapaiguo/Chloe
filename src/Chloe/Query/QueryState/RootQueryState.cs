using Chloe.DbExpressions;
using System;
using System.Linq;
using Chloe.Descriptors;
using System.Reflection;
using Chloe.InternalExtensions;
using System.Linq.Expressions;
using System.Collections.Generic;
using Chloe.Utility;
using Chloe.Infrastructure;
using Chloe.Query.QueryExpressions;

namespace Chloe.Query.QueryState
{
    internal sealed class RootQueryState : QueryStateBase
    {
        public RootQueryState(Type elementType, string explicitTableName, LockType @lock, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
           : base(CreateQueryModel(elementType, explicitTableName, @lock, scopeParameters, scopeTables))
        {
        }

        public override QueryModel ToFromQueryModel()
        {
            if (this.QueryModel.Condition == null)
            {
                QueryModel newQueryModel = new QueryModel(this.QueryModel.ScopeParameters, this.QueryModel.ScopeTables, this.QueryModel.IgnoreFilters);
                newQueryModel.FromTable = this.QueryModel.FromTable;
                newQueryModel.ResultModel = this.QueryModel.ResultModel;
                return newQueryModel;
            }

            return base.ToFromQueryModel();
        }

        public override IQueryState Accept(IncludeExpression exp)
        {
            ComplexObjectModel owner = (ComplexObjectModel)this.QueryModel.ResultModel;
            owner.Include(exp.NavigationNode, this.QueryModel, false);

            return this;
        }

        static QueryModel CreateQueryModel(Type type, string explicitTableName, LockType lockType, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            if (type.IsAbstract || type.IsInterface)
                throw new ArgumentException("The type of input can not be abstract class or interface.");

            QueryModel queryModel = new QueryModel(scopeParameters, scopeTables);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(type);

            DbTable dbTable = typeDescriptor.Table;
            if (explicitTableName != null)
                dbTable = new DbTable(explicitTableName, dbTable.Schema);
            string alias = queryModel.GenerateUniqueTableAlias(dbTable.Name);

            queryModel.FromTable = CreateRootTable(dbTable, alias, lockType);

            DbTable aliasTable = new DbTable(alias);
            ComplexObjectModel model = typeDescriptor.GenObjectModel(aliasTable);
            model.DependentTable = queryModel.FromTable;

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
