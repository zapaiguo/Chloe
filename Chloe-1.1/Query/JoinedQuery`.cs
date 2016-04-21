using Chloe.Core;
using Chloe.Database;
using Chloe.DbExpressions;
using Chloe.Infrastructure;
using Chloe.Query.QueryExpressions;
using Chloe.Query.QueryState;
using Chloe.Query.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    class JoinedQueryInfo
    {
        public JoinedQueryInfo(QueryBase query, JoinType joinType, LambdaExpression condition)
        {
            this.Query = query;
            this.JoinType = joinType;
            this.Condition = condition;
        }
        public QueryBase Query { get; set; }
        public JoinType JoinType { get; set; }
        public LambdaExpression Condition { get; set; }
    }



    class JoinedQuery<T1, T2> : IJoinedQuery<T1, T2>
    {
        InternalDbSession _dbSession;
        IDbServiceProvider _dbServiceProvider;

        QueryBase _rootQuery;
        List<JoinedQueryInfo> _joinedQueries;

        public QueryBase RootQuery { get { return this._rootQuery; } }
        public List<JoinedQueryInfo> JoinedQueries { get { return this._joinedQueries; } }

        public JoinedQuery(InternalDbSession dbSession, IDbServiceProvider dbServiceProvider, Query<T1> q1, Query<T2> q2, JoinType joinType, Expression<Func<T1, T2, bool>> on)
        {
            this._dbSession = dbSession;
            this._dbServiceProvider = dbServiceProvider;
            this._rootQuery = q1;
            this._joinedQueries = new List<JoinedQueryInfo>(1);

            JoinedQueryInfo joinedQueryInfo = new JoinedQueryInfo(q2, joinType, on);
            this._joinedQueries.Add(joinedQueryInfo);
        }

        public IJoinedQuery<T1, T2, T3> InnerJoin<T3>(IQuery<T3> q, Expression<Func<T1, T2, T3, bool>> on)
        {
            throw new NotImplementedException();
        }
        public IJoinedQuery<T1, T2, T3> LeftJoin<T3>(IQuery<T3> q, Expression<Func<T1, T2, T3, bool>> on)
        {
            throw new NotImplementedException();
        }
        public IJoinedQuery<T1, T2, T3> RightJoin<T3>(IQuery<T3> q, Expression<Func<T1, T2, T3, bool>> on)
        {
            throw new NotImplementedException();
        }
        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, TResult>> selector)
        {
            JoinQueryExpression e = new JoinQueryExpression(this._rootQuery, this._joinedQueries, selector);
            return new Query<TResult>(this._dbSession, this._dbServiceProvider, e);
        }
    }
}
