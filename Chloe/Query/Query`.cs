using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Chloe.Core;
using Chloe.Query.QueryExpressions;
using Chloe.Infrastructure;
using Chloe.Query.Internals;
using System.Diagnostics;
using Chloe.Utility;
using System.Reflection;
using Chloe.DbExpressions;

namespace Chloe.Query
{
    class Query<T> : QueryBase, IQuery<T>
    {
        static readonly List<Expression> EmptyParameterList = new List<Expression>(0);

        DbContext _dbContext;
        QueryExpression _expression;

        internal bool _trackEntity = false;
        public DbContext DbContext { get { return this._dbContext; } }

        public Query(DbContext dbContext)
            : this(dbContext, new RootQueryExpression(typeof(T)), false)
        {

        }
        public Query(DbContext dbContext, QueryExpression exp)
            : this(dbContext, exp, false)
        {
        }
        public Query(DbContext dbContext, QueryExpression exp, bool trackEntity)
        {
            this._dbContext = dbContext;
            this._expression = exp;
            this._trackEntity = trackEntity;
        }

        public IQuery<TResult> Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            Utils.CheckNull(selector);
            SelectExpression e = new SelectExpression(typeof(TResult), _expression, selector);
            return new Query<TResult>(this._dbContext, e, this._trackEntity);
        }

        public IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            Utils.CheckNull(predicate);
            WhereExpression e = new WhereExpression(_expression, typeof(T), predicate);
            return new Query<T>(this._dbContext, e, this._trackEntity);
        }
        public IOrderedQuery<T> OrderBy<K>(Expression<Func<T, K>> predicate)
        {
            Utils.CheckNull(predicate);
            OrderExpression e = new OrderExpression(QueryExpressionType.OrderBy, typeof(T), this._expression, predicate);
            return new OrderedQuery<T>(this._dbContext, e, this._trackEntity);
        }
        public IOrderedQuery<T> OrderByDesc<K>(Expression<Func<T, K>> predicate)
        {
            Utils.CheckNull(predicate);
            OrderExpression e = new OrderExpression(QueryExpressionType.OrderByDesc, typeof(T), this._expression, predicate);
            return new OrderedQuery<T>(this._dbContext, e, this._trackEntity);
        }
        public IQuery<T> Skip(int count)
        {
            SkipExpression e = new SkipExpression(typeof(T), this._expression, count);
            return new Query<T>(this._dbContext, e, this._trackEntity);
        }
        public IQuery<T> Take(int count)
        {
            TakeExpression e = new TakeExpression(typeof(T), this._expression, count);
            return new Query<T>(this._dbContext, e, this._trackEntity);
        }

        public IGroupingQuery<T> GroupBy<K>(Expression<Func<T, K>> predicate)
        {
            Utils.CheckNull(predicate);
            return new GroupingQuery<T>(this, predicate);
        }

        public IJoiningQuery<T, TSource> InnerJoin<TSource>(IQuery<TSource> q, Expression<Func<T, TSource, bool>> on)
        {
            Utils.CheckNull(q);
            Utils.CheckNull(on);
            return new JoiningQuery<T, TSource>(this, (Query<TSource>)q, JoinType.InnerJoin, on);
        }
        public IJoiningQuery<T, TSource> LeftJoin<TSource>(IQuery<TSource> q, Expression<Func<T, TSource, bool>> on)
        {
            Utils.CheckNull(q);
            Utils.CheckNull(on);
            return new JoiningQuery<T, TSource>(this, (Query<TSource>)q, JoinType.LeftJoin, on);
        }
        public IJoiningQuery<T, TSource> RightJoin<TSource>(IQuery<TSource> q, Expression<Func<T, TSource, bool>> on)
        {
            Utils.CheckNull(q);
            Utils.CheckNull(on);
            return new JoiningQuery<T, TSource>(this, (Query<TSource>)q, JoinType.RightJoin, on);
        }
        public IJoiningQuery<T, TSource> FullJoin<TSource>(IQuery<TSource> q, Expression<Func<T, TSource, bool>> on)
        {
            Utils.CheckNull(q);
            Utils.CheckNull(on);
            return new JoiningQuery<T, TSource>(this, (Query<TSource>)q, JoinType.FullJoin, on);
        }

        public T First()
        {
            var q = (Query<T>)this.Take(1);
            IEnumerable<T> iterator = q.GenenateIterator();
            return iterator.First();
        }
        public T First(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).First();
        }
        public T FirstOrDefault()
        {
            var q = (Query<T>)this.Take(1);
            IEnumerable<T> iterator = q.GenenateIterator();
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

        public bool Any()
        {
            var q = (Query<string>)this.Select(a => "1").Take(1);
            return q.GenenateIterator().Any();
        }
        public bool Any(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).Any();
        }

        public int Count()
        {
            IEnumerable<int> iterator = this.CreateAggregateQuery<int>((MethodInfo)MethodBase.GetCurrentMethod(), EmptyParameterList);
            return iterator.Single();
        }
        public long LongCount()
        {
            IEnumerable<long> iterator = this.CreateAggregateQuery<long>((MethodInfo)MethodBase.GetCurrentMethod(), EmptyParameterList);
            return iterator.Single();
        }

        public int Sum(Expression<Func<T, int>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<int> iterator = this.CreateAggregateQuery<int>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public int? Sum(Expression<Func<T, int?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<int?> iterator = this.CreateAggregateQuery<int?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long Sum(Expression<Func<T, long>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<long> iterator = this.CreateAggregateQuery<long>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long? Sum(Expression<Func<T, long?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<long?> iterator = this.CreateAggregateQuery<long?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal Sum(Expression<Func<T, decimal>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<decimal> iterator = this.CreateAggregateQuery<decimal>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal? Sum(Expression<Func<T, decimal?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<decimal?> iterator = this.CreateAggregateQuery<decimal?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double Sum(Expression<Func<T, double>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double> iterator = this.CreateAggregateQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Sum(Expression<Func<T, double?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double?> iterator = this.CreateAggregateQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float Sum(Expression<Func<T, float>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<float> iterator = this.CreateAggregateQuery<float>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float? Sum(Expression<Func<T, float?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<float?> iterator = this.CreateAggregateQuery<float?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }

        public int Max(Expression<Func<T, int>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<int> iterator = this.CreateAggregateQuery<int>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public int? Max(Expression<Func<T, int?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<int?> iterator = this.CreateAggregateQuery<int?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long Max(Expression<Func<T, long>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<long> iterator = this.CreateAggregateQuery<long>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long? Max(Expression<Func<T, long?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<long?> iterator = this.CreateAggregateQuery<long?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal Max(Expression<Func<T, decimal>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<decimal> iterator = this.CreateAggregateQuery<decimal>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal? Max(Expression<Func<T, decimal?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<decimal?> iterator = this.CreateAggregateQuery<decimal?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double Max(Expression<Func<T, double>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double> iterator = this.CreateAggregateQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Max(Expression<Func<T, double?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double?> iterator = this.CreateAggregateQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float Max(Expression<Func<T, float>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<float> iterator = this.CreateAggregateQuery<float>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float? Max(Expression<Func<T, float?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<float?> iterator = this.CreateAggregateQuery<float?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }

        public int Min(Expression<Func<T, int>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<int> iterator = this.CreateAggregateQuery<int>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public int? Min(Expression<Func<T, int?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<int?> iterator = this.CreateAggregateQuery<int?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long Min(Expression<Func<T, long>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<long> iterator = this.CreateAggregateQuery<long>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long? Min(Expression<Func<T, long?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<long?> iterator = this.CreateAggregateQuery<long?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal Min(Expression<Func<T, decimal>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<decimal> iterator = this.CreateAggregateQuery<decimal>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal? Min(Expression<Func<T, decimal?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<decimal?> iterator = this.CreateAggregateQuery<decimal?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double Min(Expression<Func<T, double>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double> iterator = this.CreateAggregateQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Min(Expression<Func<T, double?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double?> iterator = this.CreateAggregateQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float Min(Expression<Func<T, float>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<float> iterator = this.CreateAggregateQuery<float>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float? Min(Expression<Func<T, float?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<float?> iterator = this.CreateAggregateQuery<float?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }

        public double Average(Expression<Func<T, int>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double> iterator = this.CreateAggregateQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Average(Expression<Func<T, int?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double?> iterator = this.CreateAggregateQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double Average(Expression<Func<T, long>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double> iterator = this.CreateAggregateQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Average(Expression<Func<T, long?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double?> iterator = this.CreateAggregateQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal Average(Expression<Func<T, decimal>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<decimal> iterator = this.CreateAggregateQuery<decimal>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal? Average(Expression<Func<T, decimal?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<decimal?> iterator = this.CreateAggregateQuery<decimal?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double Average(Expression<Func<T, double>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double> iterator = this.CreateAggregateQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Average(Expression<Func<T, double?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<double?> iterator = this.CreateAggregateQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float Average(Expression<Func<T, float>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<float> iterator = this.CreateAggregateQuery<float>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float? Average(Expression<Func<T, float?>> selector)
        {
            Utils.CheckNull(selector);
            IEnumerable<float?> iterator = this.CreateAggregateQuery<float?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }

        public override QueryExpression QueryExpression { get { return this._expression; } }
        public override bool TrackEntity { get { return this._trackEntity; } }

        public IQuery<T> AsTracking()
        {
            return new Query<T>(this._dbContext, this.QueryExpression, true);
        }
        public IEnumerable<T> AsEnumerable()
        {
            return this.GenenateIterator();
        }

        InternalQuery<T> GenenateIterator()
        {
            InternalQuery<T> internalQuery = new InternalQuery<T>(this);
            return internalQuery;
        }
        InternalQuery<T1> CreateAggregateQuery<T1>(MethodInfo method, List<Expression> parameters)
        {
            AggregateQueryExpression e = new AggregateQueryExpression(typeof(T1), this._expression, method, parameters);
            var q = new Query<T1>(this._dbContext, e, false);
            InternalQuery<T1> iterator = q.GenenateIterator();
            return iterator;
        }

        public override string ToString()
        {
            InternalQuery<T> internalQuery = this.GenenateIterator();
            return internalQuery.ToString();
        }
    }
}
