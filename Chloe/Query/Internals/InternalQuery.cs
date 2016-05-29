using Chloe.Core;
using Chloe.Core.Visitors;
using Chloe.Infrastructure;
using Chloe.Mapper;
using Chloe.Query.Mapping;
using Chloe.Query.QueryState;
using Chloe.Query.Visitors;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Chloe.Query.Internals
{
    class InternalQuery<T> : IEnumerable<T>, IEnumerable
    {
        Query<T> _query;

        internal InternalQuery(Query<T> query)
        {
            this._query = query;
        }

        DbCommandFactor GenerateCommandFactor()
        {
            IQueryState qs = QueryExpressionVisitor.VisitQueryExpression(this._query.QueryExpression);
            MappingData data = qs.GenerateMappingData();

            AbstractDbExpressionVisitor visitor = this._query.DbContext.DbContextServiceProvider.CreateDbExpressionVisitor();
            ISqlState sqlState = data.SqlQuery.Accept(visitor);

            IObjectActivator objectActivator;
            if (this._query._trackEntity)
                objectActivator = data.MappingEntity.CreateObjectActivator(this._query.DbContext);
            else
                objectActivator = data.MappingEntity.CreateObjectActivator();

            string cmdText = sqlState.ToSql();
            IDictionary<string, object> parameters = visitor.ParameterStorage;

            DbCommandFactor commandFactor = new DbCommandFactor(objectActivator, cmdText, parameters);
            return commandFactor;
        }

        public IEnumerator<T> GetEnumerator()
        {
            DbCommandFactor commandFactor = this.GenerateCommandFactor();
            var enumerator = QueryEnumeratorCreator.CreateEnumerator<T>(this._query.DbContext.InnerDbSession, commandFactor);
            return enumerator;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            DbCommandFactor commandFactor = this.GenerateCommandFactor();
            return InternalDbSession.AppendDbCommandInfo(commandFactor.CommandText, commandFactor.Parameters);
        }
    }
}
