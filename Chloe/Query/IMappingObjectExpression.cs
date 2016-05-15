using Chloe.Extensions;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Query.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Chloe.Utility;

namespace Chloe.Query
{
    public interface IMappingObjectExpression
    {
        IObjectActivatorCreator GenarateObjectActivatorCreator(DbSqlQueryExpression sqlQuery);
        IMappingObjectExpression ToNewObjectExpression(DbSqlQueryExpression sqlQuery, DbTable table);
        void AddConstructorParameter(ParameterInfo p, DbExpression exp);
        void AddConstructorEntityParameter(ParameterInfo p, IMappingObjectExpression exp);
        void AddMemberExpression(MemberInfo p, DbExpression exp);
        void AddNavMemberExpression(MemberInfo p, IMappingObjectExpression exp);
        DbExpression GetMemberExpression(MemberInfo memberInfo);
        IMappingObjectExpression GetNavMemberExpression(MemberInfo memberInfo);
        DbExpression GetDbExpression(MemberExpression memberExpressionDeriveParameter);
        IMappingObjectExpression GetNavMemberExpression(MemberExpression exp);

        void SetNullChecking(DbExpression exp);
    }

    public static class MappingObjectExpressionHelper
    {
        public static DbExpression TryGetOrAddNullChecking(DbSqlQueryExpression sqlQuery, DbTable table, DbExpression exp)
        {
            List<DbColumnSegmentExpression> columnList = sqlQuery.Columns;
            if (exp != null)
            {
                DbColumnSegmentExpression columnSegExp = null;

                columnSegExp = columnList.Where(a => DbExpressionEqualityComparer.EqualsCompare(a.Body, exp)).FirstOrDefault();

                if (columnSegExp == null)
                {
                    string alias = Utils.GenerateUniqueColumnAlias(sqlQuery);
                    columnSegExp = new DbColumnSegmentExpression(exp.Type, exp, alias);

                    columnList.Add(columnSegExp);
                }

                DbColumnAccessExpression cae = new DbColumnAccessExpression(columnSegExp.Type, table, columnSegExp.Alias);
                return cae;
            }

            return null;
        }
        public static int? TryGetOrAddColumn(DbSqlQueryExpression sqlQuery, DbExpression exp, string addDefaultAlias = Utils.DefaultColumnAlias)
        {
            if (exp == null)
                return null;

            List<DbColumnSegmentExpression> columnList = sqlQuery.Columns;
            DbColumnSegmentExpression columnSegExp = null;

            int? ordinal = null;
            for (int i = 0; i < columnList.Count; i++)
            {
                var item = columnList[i];
                if (DbExpressionEqualityComparer.EqualsCompare(item.Body, exp))
                {
                    ordinal = i;
                    columnSegExp = item;
                    break;
                }
            }

            if (ordinal == null)
            {
                string alias = Utils.GenerateUniqueColumnAlias(sqlQuery, addDefaultAlias);
                columnSegExp = new DbColumnSegmentExpression(exp.Type, exp, alias);

                columnList.Add(columnSegExp);
                ordinal = columnList.Count - 1;
            }

            return ordinal.Value;
        }
        public static DbColumnAccessExpression ParseColumnAccessExpression(DbSqlQueryExpression sqlQuery, DbTable table, DbExpression exp, string defaultAlias = Utils.DefaultColumnAlias)
        {
            string alias = Utils.GenerateUniqueColumnAlias(sqlQuery, defaultAlias);
            DbColumnSegmentExpression columnSegExp = new DbColumnSegmentExpression(exp.Type, exp, alias);

            sqlQuery.Columns.Add(columnSegExp);

            DbColumnAccessExpression cae = new DbColumnAccessExpression(exp.Type, table, alias);

            return cae;
        }
    }
}
