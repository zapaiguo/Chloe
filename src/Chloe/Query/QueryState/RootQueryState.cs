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
        RootQueryExpression _rootQueryExp;
        public RootQueryState(RootQueryExpression rootQueryExp, ScopeParameterDictionary scopeParameters, StringSet scopeTables) : this(rootQueryExp, scopeParameters, scopeTables, null)
        {
        }
        public RootQueryState(RootQueryExpression rootQueryExp, ScopeParameterDictionary scopeParameters, StringSet scopeTables, Func<string, string> tableAliasGenerator)
          : base(CreateQueryModel(rootQueryExp, scopeParameters, scopeTables, tableAliasGenerator))
        {
            this._rootQueryExp = rootQueryExp;
        }

        public override QueryModel ToFromQueryModel()
        {
            QueryModel newQueryModel = new QueryModel(this.QueryModel.ScopeParameters, this.QueryModel.ScopeTables, this.QueryModel.IgnoreFilters);
            newQueryModel.FromTable = this.QueryModel.FromTable;
            newQueryModel.ResultModel = this.QueryModel.ResultModel;
            if (!this.QueryModel.IgnoreFilters)
            {
                newQueryModel.Filters.AddRange(this.QueryModel.Filters);
            }

            return newQueryModel;
        }

        public override IQueryState Accept(IncludeExpression exp)
        {
            ComplexObjectModel owner = (ComplexObjectModel)this.QueryModel.ResultModel;
            owner.Include(exp.NavigationNode, this.QueryModel, false);

            return this;
        }

        public override JoinQueryResult ToJoinQueryResult(JoinType joinType, LambdaExpression conditionExpression, ScopeParameterDictionary scopeParameters, StringSet scopeTables, Func<string, string> tableAliasGenerator)
        {
            scopeParameters = scopeParameters.Clone(conditionExpression.Parameters.Last(), this.QueryModel.ResultModel);
            DbExpression condition = GeneralExpressionParser.Parse(conditionExpression, scopeParameters, scopeTables);
            DbJoinTableExpression joinTable = new DbJoinTableExpression(joinType.AsDbJoinType(), this.QueryModel.FromTable.Table, condition);

            if (!this.QueryModel.IgnoreFilters)
            {
                this.QueryModel.Filters.ForEach(a =>
                {
                    joinTable.Condition = joinTable.Condition.And(a);
                });
            }

            JoinQueryResult result = new JoinQueryResult();
            result.ResultModel = this.QueryModel.ResultModel;
            result.JoinTable = joinTable;

            return result;
        }

        static QueryModel CreateQueryModel(RootQueryExpression rootQueryExp, ScopeParameterDictionary scopeParameters, StringSet scopeTables, Func<string, string> tableAliasGenerator)
        {
            Type entityType = rootQueryExp.ElementType;

            if (entityType.IsAbstract || entityType.IsInterface)
                throw new ArgumentException("The type of input can not be abstract class or interface.");

            QueryModel queryModel = new QueryModel(scopeParameters, scopeTables);

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entityType);

            DbTable dbTable = typeDescriptor.GenDbTable(rootQueryExp.ExplicitTable);
            string alias = null;
            if (tableAliasGenerator != null)
                alias = tableAliasGenerator(dbTable.Name);
            else
                alias = queryModel.GenerateUniqueTableAlias(dbTable.Name);

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
