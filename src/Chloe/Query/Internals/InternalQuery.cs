using Chloe.Core;
using Chloe.Core.Visitors;
using Chloe.Data;
using Chloe.Infrastructure;
using Chloe.Mapper;
using Chloe.Mapper.Activators;
using Chloe.Query.Mapping;
using Chloe.Query.QueryState;
using Chloe.Query.Visitors;
using Chloe.Utility;
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
            IQueryState qs = QueryExpressionResolver.Resolve(this._query.QueryExpression, new ScopeParameterDictionary(), new StringSet());
            MappingData data = qs.GenerateMappingData();

            IObjectActivator objectActivator;
            if (this._query._trackEntity)
                objectActivator = data.ObjectActivatorCreator.CreateObjectActivator(this._query.DbContext);
            else
                objectActivator = data.ObjectActivatorCreator.CreateObjectActivator();

            IDbExpressionTranslator translator = this._query.DbContext.DatabaseProvider.CreateDbExpressionTranslator();
            List<DbParam> parameters;
            string cmdText = translator.Translate(data.SqlQuery, out parameters);

            DbCommandFactor commandFactor = new DbCommandFactor(objectActivator, cmdText, parameters.ToArray());
            return commandFactor;
        }

        public IEnumerator<T> GetEnumerator()
        {
            DbCommandFactor commandFactor = this.GenerateCommandFactor();
            var enumerator = QueryEnumeratorCreator.CreateEnumerator<T>(commandFactor, cmdFactor =>
            {
                IDataReader dataReader = this._query.DbContext.AdoSession.ExecuteReader(cmdFactor.CommandText, cmdFactor.Parameters, CommandType.Text);

                if (cmdFactor.ObjectActivator is RootEntityActivator)
                {
                    dataReader = new QueryDataReader(dataReader);
                }

                cmdFactor.ObjectActivator.Prepare(dataReader);

                return dataReader;
            });
            return enumerator;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            DbCommandFactor commandFactor = this.GenerateCommandFactor();
            return InnerAdoSession.AppendDbCommandInfo(commandFactor.CommandText, commandFactor.Parameters);
        }
    }
}
