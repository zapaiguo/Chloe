using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Chloe.Core;
using Chloe.Query.QueryExpressions;
using Chloe.Infrastructure;
using Chloe.Query.Internals;
using Chloe.Database;

namespace Chloe.Query
{
    class Query<T> : IQuery<T>, IQuery
    {
        QueryExpression _expression;
        protected InternalDbSession _dbSession;
        protected IDbServiceProvider _dbServiceProvider;

        public Query(InternalDbSession dbSession, IDbServiceProvider dbServiceProvider)
            : this(dbSession, dbServiceProvider, new RootQueryExpression(typeof(T)))
        {

        }
        protected Query(InternalDbSession dbSession, IDbServiceProvider dbServiceProvider, QueryExpression exp)
        {
            this._dbSession = dbSession;
            this._dbServiceProvider = dbServiceProvider;
            this._expression = exp;
        }

        public IQuery<T1> Select<T1>(Expression<Func<T, T1>> selector)
        {
            SelectExpression e = new SelectExpression(_expression, typeof(T1), selector);
            return new Query<T1>(this._dbSession, this._dbServiceProvider, e);
        }

        public IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            WhereExpression e = new WhereExpression(_expression, typeof(T), predicate);
            return new Query<T>(this._dbSession, this._dbServiceProvider, e);
        }

        public IQuery<T> Skip(int count)
        {
            SkipExpression e = new SkipExpression(_expression, typeof(T), count);
            return new Query<T>(this._dbSession, this._dbServiceProvider, e);
        }
        public IQuery<T> Take(int count)
        {
            TakeExpression e = new TakeExpression(_expression, typeof(T), count);
            return new Query<T>(this._dbSession, this._dbServiceProvider, e);
        }

        public IOrderedQuery<T> OrderBy<K>(Expression<Func<T, K>> predicate)
        {
            OrderExpression e = new OrderExpression(QueryExpressionType.OrderBy, typeof(T), ((IQuery)this).QueryExpression, predicate);
            return new OrderedQuery<T>(this._dbSession, this._dbServiceProvider, e);
        }
        public IOrderedQuery<T> OrderByDesc<K>(Expression<Func<T, K>> predicate)
        {
            OrderExpression e = new OrderExpression(QueryExpressionType.OrderByDesc, typeof(T), ((IQuery)this).QueryExpression, predicate);
            return new OrderedQuery<T>(this._dbSession, this._dbServiceProvider, e);
        }

        public T FirstOrDefault()
        {
            IEnumerable<T> iterator = this.GenenateIterator();
            return iterator.FirstOrDefault();
        }
        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).FirstOrDefault();
        }
        public List<T> ToList()
        {
            IEnumerable<T> iterator = this.GenenateIterator();
            return iterator.ToList();
        }

        public bool Exists()
        {
            throw new NotImplementedException();
        }
        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }
        public long LongCount()
        {
            throw new NotImplementedException();
        }

        public int Sum(Expression<Func<T, int>> selector)
        {
            throw new NotImplementedException();
        }
        public int? Sum(Expression<Func<T, int?>> selector)
        {
            throw new NotImplementedException();
        }
        public long Sum(Expression<Func<T, long>> selector)
        {
            throw new NotImplementedException();
        }
        public long? Sum(Expression<Func<T, long?>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal Sum(Expression<Func<T, decimal>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal? Sum(Expression<Func<T, decimal?>> selector)
        {
            throw new NotImplementedException();
        }
        public double Sum(Expression<Func<T, double>> selector)
        {
            throw new NotImplementedException();
        }
        public double? Sum(Expression<Func<T, double?>> selector)
        {
            throw new NotImplementedException();
        }
        public float Sum(Expression<Func<T, float>> selector)
        {
            throw new NotImplementedException();
        }
        public float? Sum(Expression<Func<T, float?>> selector)
        {
            throw new NotImplementedException();
        }

        public int Max(Expression<Func<T, int>> selector)
        {
            throw new NotImplementedException();
        }
        public int? Max(Expression<Func<T, int?>> selector)
        {
            throw new NotImplementedException();
        }
        public long Max(Expression<Func<T, long>> selector)
        {
            throw new NotImplementedException();
        }
        public long? Max(Expression<Func<T, long?>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal Max(Expression<Func<T, decimal>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal? Max(Expression<Func<T, decimal?>> selector)
        {
            throw new NotImplementedException();
        }
        public double Max(Expression<Func<T, double>> selector)
        {
            throw new NotImplementedException();
        }
        public double? Max(Expression<Func<T, double?>> selector)
        {
            throw new NotImplementedException();
        }
        public float Max(Expression<Func<T, float>> selector)
        {
            throw new NotImplementedException();
        }
        public float? Max(Expression<Func<T, float?>> selector)
        {
            throw new NotImplementedException();
        }

        public int Min(Expression<Func<T, int>> selector)
        {
            throw new NotImplementedException();
        }
        public int? Min(Expression<Func<T, int?>> selector)
        {
            throw new NotImplementedException();
        }
        public long Min(Expression<Func<T, long>> selector)
        {
            throw new NotImplementedException();
        }
        public long? Min(Expression<Func<T, long?>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal Min(Expression<Func<T, decimal>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal? Min(Expression<Func<T, decimal?>> selector)
        {
            throw new NotImplementedException();
        }
        public double Min(Expression<Func<T, double>> selector)
        {
            throw new NotImplementedException();
        }
        public double? Min(Expression<Func<T, double?>> selector)
        {
            throw new NotImplementedException();
        }
        public float Min(Expression<Func<T, float>> selector)
        {
            throw new NotImplementedException();
        }
        public float? Min(Expression<Func<T, float?>> selector)
        {
            throw new NotImplementedException();
        }

        public double Average(Expression<Func<T, int>> selector)
        {
            throw new NotImplementedException();
        }
        public double? Average(Expression<Func<T, int?>> selector)
        {
            throw new NotImplementedException();
        }
        public double Average(Expression<Func<T, long>> selector)
        {
            throw new NotImplementedException();
        }
        public double? Average(Expression<Func<T, long?>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal Average(Expression<Func<T, decimal>> selector)
        {
            throw new NotImplementedException();
        }
        public decimal? Average(Expression<Func<T, decimal?>> selector)
        {
            throw new NotImplementedException();
        }
        public double Average(Expression<Func<T, double>> selector)
        {
            throw new NotImplementedException();
        }
        public double? Average(Expression<Func<T, double?>> selector)
        {
            throw new NotImplementedException();
        }
        public float Average(Expression<Func<T, float>> selector)
        {
            throw new NotImplementedException();
        }
        public float? Average(Expression<Func<T, float?>> selector)
        {
            throw new NotImplementedException();
        }

        public QueryExpression QueryExpression
        {
            get { return _expression; }
        }


        IEnumerable<T> GenenateIterator()
        {
            InternalQuery<T> internalQuery = new InternalQuery<T>(this, this._dbSession, this._dbServiceProvider);
            return internalQuery;
        }
    }

    internal class OrderedQuery<T> : Query<T>, IOrderedQuery<T>
    {
        public OrderedQuery(InternalDbSession dbSession, IDbServiceProvider dbServiceProvider, QueryExpression exp)
            : base(dbSession, dbServiceProvider, exp)
        {

        }
        public IOrderedQuery<T> ThenBy<K>(Expression<Func<T, K>> predicate)
        {
            OrderExpression e = new OrderExpression(QueryExpressionType.ThenBy, typeof(T), ((IQuery)this).QueryExpression, predicate);
            return new OrderedQuery<T>(this._dbSession, this._dbServiceProvider, e);
        }
        public IOrderedQuery<T> ThenByDesc<K>(Expression<Func<T, K>> predicate)
        {
            OrderExpression e = new OrderExpression(QueryExpressionType.ThenByDesc, typeof(T), ((IQuery)this).QueryExpression, predicate);
            return new OrderedQuery<T>(this._dbSession, this._dbServiceProvider, e);
        }
    }
}
