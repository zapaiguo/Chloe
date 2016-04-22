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
    class JoinedQuery<T1, T2> : IJoinedQuery<T1, T2>
    {
        InternalDbSession _dbSession;
        IDbServiceProvider _dbServiceProvider;

        QueryBase _rootQuery;
        List<JoinedQueryInfo> _joinedQueries;

        public InternalDbSession DbSession { get { return this._dbSession; } }
        public IDbServiceProvider DbServiceProvider { get { return this._dbServiceProvider; } }
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
            return new JoinedQuery<T1, T2, T3>(this, (Query<T3>)q, JoinType.InnerJoin, on);
        }
        public IJoinedQuery<T1, T2, T3> LeftJoin<T3>(IQuery<T3> q, Expression<Func<T1, T2, T3, bool>> on)
        {
            return new JoinedQuery<T1, T2, T3>(this, (Query<T3>)q, JoinType.LeftJoin, on);
        }
        public IJoinedQuery<T1, T2, T3> RightJoin<T3>(IQuery<T3> q, Expression<Func<T1, T2, T3, bool>> on)
        {
            return new JoinedQuery<T1, T2, T3>(this, (Query<T3>)q, JoinType.RightJoin, on);
        }
        public IJoinedQuery<T1, T2, T3> FullJoin<T3>(IQuery<T3> q, Expression<Func<T1, T2, T3, bool>> on)
        {
            return new JoinedQuery<T1, T2, T3>(this, (Query<T3>)q, JoinType.FullJoin, on);
        }
        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, TResult>> selector)
        {
            JoinQueryExpression e = new JoinQueryExpression(this._rootQuery, this._joinedQueries, selector);
            return new Query<TResult>(this._dbSession, this._dbServiceProvider, e);
        }
    }

    class JoinedQuery<T1, T2, T3> : IJoinedQuery<T1, T2, T3>
    {
        InternalDbSession _dbSession;
        IDbServiceProvider _dbServiceProvider;

        QueryBase _rootQuery;
        List<JoinedQueryInfo> _joinedQueries;

        public InternalDbSession DbSession { get { return this._dbSession; } }
        public IDbServiceProvider DbServiceProvider { get { return this._dbServiceProvider; } }
        public QueryBase RootQuery { get { return this._rootQuery; } }
        public List<JoinedQueryInfo> JoinedQueries { get { return this._joinedQueries; } }

        public JoinedQuery(JoinedQuery<T1, T2> joinedQuery, Query<T3> q, JoinType joinType, Expression<Func<T1, T2, T3, bool>> on)
        {
            this._dbSession = joinedQuery.DbSession;
            this._dbServiceProvider = joinedQuery.DbServiceProvider;
            this._rootQuery = joinedQuery.RootQuery;
            this._joinedQueries = new List<JoinedQueryInfo>(joinedQuery.JoinedQueries.Count);

            this._joinedQueries.AddRange(joinedQuery.JoinedQueries);

            JoinedQueryInfo joinedQueryInfo = new JoinedQueryInfo(q, joinType, on);
            this._joinedQueries.Add(joinedQueryInfo);
        }

        public IJoinedQuery<T1, T2, T3, T4> InnerJoin<T4>(IQuery<T4> q, Expression<Func<T1, T2, T3, T4, bool>> on)
        {
            return new JoinedQuery<T1, T2, T3, T4>(this, (Query<T4>)q, JoinType.InnerJoin, on);
        }
        public IJoinedQuery<T1, T2, T3, T4> LeftJoin<T4>(IQuery<T4> q, Expression<Func<T1, T2, T3, T4, bool>> on)
        {
            return new JoinedQuery<T1, T2, T3, T4>(this, (Query<T4>)q, JoinType.LeftJoin, on);
        }
        public IJoinedQuery<T1, T2, T3, T4> RightJoin<T4>(IQuery<T4> q, Expression<Func<T1, T2, T3, T4, bool>> on)
        {
            return new JoinedQuery<T1, T2, T3, T4>(this, (Query<T4>)q, JoinType.RightJoin, on);
        }
        public IJoinedQuery<T1, T2, T3, T4> FullJoin<T4>(IQuery<T4> q, Expression<Func<T1, T2, T3, T4, bool>> on)
        {
            return new JoinedQuery<T1, T2, T3, T4>(this, (Query<T4>)q, JoinType.FullJoin, on);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, TResult>> selector)
        {
            JoinQueryExpression e = new JoinQueryExpression(this._rootQuery, this._joinedQueries, selector);
            return new Query<TResult>(this._dbSession, this._dbServiceProvider, e);
        }
    }

    class JoinedQuery<T1, T2, T3, T4> : IJoinedQuery<T1, T2, T3, T4>
    {
        InternalDbSession _dbSession;
        IDbServiceProvider _dbServiceProvider;

        QueryBase _rootQuery;
        List<JoinedQueryInfo> _joinedQueries;

        public InternalDbSession DbSession { get { return this._dbSession; } }
        public IDbServiceProvider DbServiceProvider { get { return this._dbServiceProvider; } }
        public QueryBase RootQuery { get { return this._rootQuery; } }
        public List<JoinedQueryInfo> JoinedQueries { get { return this._joinedQueries; } }

        public JoinedQuery(JoinedQuery<T1, T2, T3> joinedQuery, Query<T4> q, JoinType joinType, Expression<Func<T1, T2, T3, T4, bool>> on)
        {
            this._dbSession = joinedQuery.DbSession;
            this._dbServiceProvider = joinedQuery.DbServiceProvider;
            this._rootQuery = joinedQuery.RootQuery;
            this._joinedQueries = new List<JoinedQueryInfo>(joinedQuery.JoinedQueries.Count);

            this._joinedQueries.AddRange(joinedQuery.JoinedQueries);

            JoinedQueryInfo joinedQueryInfo = new JoinedQueryInfo(q, joinType, on);
            this._joinedQueries.Add(joinedQueryInfo);
        }

        public IJoinedQuery<T1, T2, T3, T4, T5> InnerJoin<T5>(IQuery<T5> q, Expression<Func<T1, T2, T3, T4, T5, bool>> on)
        {
            return new JoinedQuery<T1, T2, T3, T4, T5>(this, (Query<T5>)q, JoinType.InnerJoin, on);
        }
        public IJoinedQuery<T1, T2, T3, T4, T5> LeftJoin<T5>(IQuery<T5> q, Expression<Func<T1, T2, T3, T4, T5, bool>> on)
        {
            return new JoinedQuery<T1, T2, T3, T4, T5>(this, (Query<T5>)q, JoinType.LeftJoin, on);
        }
        public IJoinedQuery<T1, T2, T3, T4, T5> RightJoin<T5>(IQuery<T5> q, Expression<Func<T1, T2, T3, T4, T5, bool>> on)
        {
            return new JoinedQuery<T1, T2, T3, T4, T5>(this, (Query<T5>)q, JoinType.RightJoin, on);
        }
        public IJoinedQuery<T1, T2, T3, T4, T5> FullJoin<T5>(IQuery<T5> q, Expression<Func<T1, T2, T3, T4, T5, bool>> on)
        {
            return new JoinedQuery<T1, T2, T3, T4, T5>(this, (Query<T5>)q, JoinType.FullJoin, on);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, TResult>> selector)
        {
            JoinQueryExpression e = new JoinQueryExpression(this._rootQuery, this._joinedQueries, selector);
            return new Query<TResult>(this._dbSession, this._dbServiceProvider, e);
        }
    }

    class JoinedQuery<T1, T2, T3, T4, T5> : IJoinedQuery<T1, T2, T3, T4, T5>
    {
        InternalDbSession _dbSession;
        IDbServiceProvider _dbServiceProvider;

        QueryBase _rootQuery;
        List<JoinedQueryInfo> _joinedQueries;

        public InternalDbSession DbSession { get { return this._dbSession; } }
        public IDbServiceProvider DbServiceProvider { get { return this._dbServiceProvider; } }
        public QueryBase RootQuery { get { return this._rootQuery; } }
        public List<JoinedQueryInfo> JoinedQueries { get { return this._joinedQueries; } }

        public JoinedQuery(JoinedQuery<T1, T2, T3, T4> joinedQuery, Query<T5> q, JoinType joinType, Expression<Func<T1, T2, T3, T4, T5, bool>> on)
        {
            this._dbSession = joinedQuery.DbSession;
            this._dbServiceProvider = joinedQuery.DbServiceProvider;
            this._rootQuery = joinedQuery.RootQuery;
            this._joinedQueries = new List<JoinedQueryInfo>(joinedQuery.JoinedQueries.Count);

            this._joinedQueries.AddRange(joinedQuery.JoinedQueries);

            JoinedQueryInfo joinedQueryInfo = new JoinedQueryInfo(q, joinType, on);
            this._joinedQueries.Add(joinedQueryInfo);
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T1, T2, T3, T4, T5, TResult>> selector)
        {
            JoinQueryExpression e = new JoinQueryExpression(this._rootQuery, this._joinedQueries, selector);
            return new Query<TResult>(this._dbSession, this._dbServiceProvider, e);
        }
    }

}
