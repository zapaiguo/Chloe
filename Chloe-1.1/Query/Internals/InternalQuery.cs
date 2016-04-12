using Chloe.Core;
using Chloe.Database;
using Chloe.Mapper;
using Chloe.Query.Implementation;
using Chloe.Query.Mapping;
using Chloe.Query.QueryExpressions;
using Chloe.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Internals
{
    public class InternalQuery<T> : IEnumerable<T>, IEnumerable
    {
        IQuery<T> _query;
        InternalDbSession _dbSession;

        internal InternalQuery(IQuery<T> query, InternalDbSession dbSession)
        {
            this._query = query;
            this._dbSession = dbSession;
        }

        QueryFactor GenerateQueryComponent()
        {
            DbExpressionVisitorBase visitor = SqlExpressionVisitor.CreateInstance();
            IQueryState qs = QueryExpressionReducer.ReduceQueryExpression(this._query.QueryExpression);
            MappingData data = qs.GenerateMappingData();
            ISqlState sqlState = data.SqlQuery.Accept(visitor);

            IObjectActivtor objectActivtor = data.MappingEntity.CreateObjectActivtor();
            string cmdText = sqlState.ToSql();
            IDictionary<string, object> parameters = visitor.ParameterStorage;

            QueryFactor queryFactor = new QueryFactor(objectActivtor, cmdText, parameters);
            return queryFactor;
        }

        public IEnumerator<T> GetEnumerator()
        {
            QueryFactor queryFactor = this.GenerateQueryComponent();

#if DEBUG
            Debug.WriteLine(queryFactor.CmdText);
#endif

            var enumerator = QueryEnumeratorCreator.CreateEnumerator<T>(this._dbSession, queryFactor);
            return enumerator;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            QueryFactor queryFactor = this.GenerateQueryComponent();
            return queryFactor.CmdText;
        }
    }

    public class InternalQueryHelper
    {
        public static InternalQuery<T> CreateQuery<T>(IQuery<T> query, IDbConnection conn)
        {
            return new InternalQuery<T>(query, new InternalDbSession(conn));
        }
    }
}
