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
using Chloe.Query.Visitors;

namespace Chloe.Query.QueryState
{
    internal sealed class RootQueryState : QueryStateBase
    {
        public RootQueryState(RootQueryExpression rootQueryExp, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
           : base(CreateQueryModel(rootQueryExp, scopeParameters, scopeTables))
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

        static QueryModel CreateQueryModel(RootQueryExpression rootQueryExp, ScopeParameterDictionary scopeParameters, StringSet scopeTables)
        {
            Type entityType = rootQueryExp.ElementType;

            if (entityType.IsAbstract || entityType.IsInterface)
                throw new ArgumentException("The type of input can not be abstract class or interface.");

            QueryModel queryModel = new QueryModel(scopeParameters, scopeTables);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entityType);

            DbTable dbTable = typeDescriptor.Table;
            if (rootQueryExp.ExplicitTable != null)
                dbTable = new DbTable(rootQueryExp.ExplicitTable, dbTable.Schema);
            string alias = queryModel.GenerateUniqueTableAlias(dbTable.Name);

            queryModel.FromTable = CreateRootTable(dbTable, alias, rootQueryExp.Lock);

            DbTable aliasTable = new DbTable(alias);
            ComplexObjectModel model = typeDescriptor.GenObjectModel(aliasTable);
            model.DependentTable = queryModel.FromTable;

            queryModel.ResultModel = model;

            ParseFilters(queryModel, typeDescriptor.Definition.Filters.Concat(rootQueryExp.InstanceFilters));

            return queryModel;
        }
        static DbFromTableExpression CreateRootTable(DbTable table, string alias, LockType lockType)
        {
            DbTableExpression tableExp = new DbTableExpression(table);
            DbTableSegment tableSeg = new DbTableSegment(tableExp, alias, lockType);
            var fromTableExp = new DbFromTableExpression(tableSeg);
            return fromTableExp;
        }
        static void ParseFilters(QueryModel queryModel, IEnumerable<LambdaExpression> filters)
        {
            foreach (var filter in filters)
            {
                ScopeParameterDictionary scopeParameters = queryModel.ScopeParameters.Clone(filter.Parameters[0], queryModel.ResultModel);
                DbExpression filterCondition = FilterPredicateParser.Parse(filter, scopeParameters, queryModel.ScopeTables);
                queryModel.Filters.Add(filterCondition);
            }
        }
    }
}
