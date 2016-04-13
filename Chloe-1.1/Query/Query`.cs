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
using System.Diagnostics;
using Chloe.Utility;
using System.Reflection;

namespace Chloe.Query
{
    class Query<T> : IQuery<T>//, IQuery
    {
        static readonly List<Expression> EmptyParameterList = new List<Expression>(0);

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
            SelectExpression e = new SelectExpression(typeof(T1), _expression, selector);
            return new Query<T1>(this._dbSession, this._dbServiceProvider, e);
        }

        public IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            WhereExpression e = new WhereExpression(_expression, typeof(T), predicate);
            return new Query<T>(this._dbSession, this._dbServiceProvider, e);
        }

        public IQuery<T> Skip(int count)
        {
            SkipExpression e = new SkipExpression(typeof(T), this._expression, count);
            return new Query<T>(this._dbSession, this._dbServiceProvider, e);
        }
        public IQuery<T> Take(int count)
        {
            TakeExpression e = new TakeExpression(typeof(T), this._expression, count);
            return new Query<T>(this._dbSession, this._dbServiceProvider, e);
        }

        public IOrderedQuery<T> OrderBy<K>(Expression<Func<T, K>> predicate)
        {
            OrderExpression e = new OrderExpression(QueryExpressionType.OrderBy, typeof(T), this._expression, predicate);
            return new OrderedQuery<T>(this._dbSession, this._dbServiceProvider, e);
        }
        public IOrderedQuery<T> OrderByDesc<K>(Expression<Func<T, K>> predicate)
        {
            OrderExpression e = new OrderExpression(QueryExpressionType.OrderByDesc, typeof(T), this._expression, predicate);
            return new OrderedQuery<T>(this._dbSession, this._dbServiceProvider, e);
        }

        public T First()
        {
            var q = (Query<T>)this.Take(1);
            IEnumerable<T> iterator = q.GenenateIterator();
            return iterator.First();
        }

        public T FirstOrDefault()
        {
            var q = (Query<T>)this.Take(1);
            IEnumerable<T> iterator = q.GenenateIterator();
            return iterator.FirstOrDefault();
        }
        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            var q = (Query<T>)this.Where(predicate).Take(1);
            IEnumerable<T> iterator = q.GenenateIterator();
            return iterator.FirstOrDefault();
        }
        public List<T> ToList()
        {
            IEnumerable<T> iterator = this.GenenateIterator();
            return iterator.ToList();
        }

        public bool Exists()
        {
            var q = (Query<int>)this.Select(a => 1).Take(1);
            return q.GenenateIterator().Any();
        }
        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            return this.Where(predicate).Exists();
        }

        public int Count()
        {
            IEnumerable<int> iterator = this.CreateFunctionQuery<int>((MethodInfo)MethodBase.GetCurrentMethod(), EmptyParameterList);
            return iterator.Single();
        }
        public long LongCount()
        {
            IEnumerable<long> iterator = this.CreateFunctionQuery<long>((MethodInfo)MethodBase.GetCurrentMethod(), EmptyParameterList);
            return iterator.Single();
        }

        public int Sum(Expression<Func<T, int>> selector)
        {
            IEnumerable<int> iterator = this.CreateFunctionQuery<int>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public int? Sum(Expression<Func<T, int?>> selector)
        {
            IEnumerable<int?> iterator = this.CreateFunctionQuery<int?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long Sum(Expression<Func<T, long>> selector)
        {
            IEnumerable<long> iterator = this.CreateFunctionQuery<long>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long? Sum(Expression<Func<T, long?>> selector)
        {
            IEnumerable<long?> iterator = this.CreateFunctionQuery<long?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal Sum(Expression<Func<T, decimal>> selector)
        {
            IEnumerable<decimal> iterator = this.CreateFunctionQuery<decimal>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal? Sum(Expression<Func<T, decimal?>> selector)
        {
            IEnumerable<decimal?> iterator = this.CreateFunctionQuery<decimal?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double Sum(Expression<Func<T, double>> selector)
        {
            IEnumerable<double> iterator = this.CreateFunctionQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Sum(Expression<Func<T, double?>> selector)
        {
            IEnumerable<double?> iterator = this.CreateFunctionQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float Sum(Expression<Func<T, float>> selector)
        {
            IEnumerable<float> iterator = this.CreateFunctionQuery<float>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float? Sum(Expression<Func<T, float?>> selector)
        {
            IEnumerable<float?> iterator = this.CreateFunctionQuery<float?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }

        public int Max(Expression<Func<T, int>> selector)
        {
            IEnumerable<int> iterator = this.CreateFunctionQuery<int>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public int? Max(Expression<Func<T, int?>> selector)
        {
            IEnumerable<int?> iterator = this.CreateFunctionQuery<int?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long Max(Expression<Func<T, long>> selector)
        {
            IEnumerable<long> iterator = this.CreateFunctionQuery<long>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long? Max(Expression<Func<T, long?>> selector)
        {
            IEnumerable<long?> iterator = this.CreateFunctionQuery<long?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal Max(Expression<Func<T, decimal>> selector)
        {
            IEnumerable<decimal> iterator = this.CreateFunctionQuery<decimal>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal? Max(Expression<Func<T, decimal?>> selector)
        {
            IEnumerable<decimal?> iterator = this.CreateFunctionQuery<decimal?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double Max(Expression<Func<T, double>> selector)
        {
            IEnumerable<double> iterator = this.CreateFunctionQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Max(Expression<Func<T, double?>> selector)
        {
            IEnumerable<double?> iterator = this.CreateFunctionQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float Max(Expression<Func<T, float>> selector)
        {
            IEnumerable<float> iterator = this.CreateFunctionQuery<float>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float? Max(Expression<Func<T, float?>> selector)
        {
            IEnumerable<float?> iterator = this.CreateFunctionQuery<float?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }

        public int Min(Expression<Func<T, int>> selector)
        {
            IEnumerable<int> iterator = this.CreateFunctionQuery<int>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public int? Min(Expression<Func<T, int?>> selector)
        {
            IEnumerable<int?> iterator = this.CreateFunctionQuery<int?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long Min(Expression<Func<T, long>> selector)
        {
            IEnumerable<long> iterator = this.CreateFunctionQuery<long>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public long? Min(Expression<Func<T, long?>> selector)
        {
            IEnumerable<long?> iterator = this.CreateFunctionQuery<long?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal Min(Expression<Func<T, decimal>> selector)
        {
            IEnumerable<decimal> iterator = this.CreateFunctionQuery<decimal>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal? Min(Expression<Func<T, decimal?>> selector)
        {
            IEnumerable<decimal?> iterator = this.CreateFunctionQuery<decimal?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double Min(Expression<Func<T, double>> selector)
        {
            IEnumerable<double> iterator = this.CreateFunctionQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Min(Expression<Func<T, double?>> selector)
        {
            IEnumerable<double?> iterator = this.CreateFunctionQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float Min(Expression<Func<T, float>> selector)
        {
            IEnumerable<float> iterator = this.CreateFunctionQuery<float>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float? Min(Expression<Func<T, float?>> selector)
        {
            IEnumerable<float?> iterator = this.CreateFunctionQuery<float?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }

        public double Average(Expression<Func<T, int>> selector)
        {
            IEnumerable<double> iterator = this.CreateFunctionQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Average(Expression<Func<T, int?>> selector)
        {
            IEnumerable<double?> iterator = this.CreateFunctionQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double Average(Expression<Func<T, long>> selector)
        {
            IEnumerable<double> iterator = this.CreateFunctionQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Average(Expression<Func<T, long?>> selector)
        {
            IEnumerable<double?> iterator = this.CreateFunctionQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal Average(Expression<Func<T, decimal>> selector)
        {
            IEnumerable<decimal> iterator = this.CreateFunctionQuery<decimal>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public decimal? Average(Expression<Func<T, decimal?>> selector)
        {
            IEnumerable<decimal?> iterator = this.CreateFunctionQuery<decimal?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double Average(Expression<Func<T, double>> selector)
        {
            IEnumerable<double> iterator = this.CreateFunctionQuery<double>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public double? Average(Expression<Func<T, double?>> selector)
        {
            IEnumerable<double?> iterator = this.CreateFunctionQuery<double?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float Average(Expression<Func<T, float>> selector)
        {
            IEnumerable<float> iterator = this.CreateFunctionQuery<float>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }
        public float? Average(Expression<Func<T, float?>> selector)
        {
            IEnumerable<float?> iterator = this.CreateFunctionQuery<float?>((MethodInfo)MethodBase.GetCurrentMethod(), new List<Expression>() { selector });
            return iterator.Single();
        }

        public QueryExpression QueryExpression
        {
            get { return _expression; }
        }


        InternalQuery<T> GenenateIterator()
        {
            InternalQuery<T> internalQuery = new InternalQuery<T>(this, this._dbSession, this._dbServiceProvider);
            return internalQuery;
        }
        InternalQuery<T1> CreateFunctionQuery<T1>(MethodInfo method, List<Expression> parameters)
        {
            FunctionExpression e = new FunctionExpression(typeof(T1), this._expression, (MethodInfo)MethodBase.GetCurrentMethod(), parameters);
            var q = new Query<T1>(this._dbSession, this._dbServiceProvider, e);
            InternalQuery<T1> iterator = q.GenenateIterator();
            return iterator;
        }

        public override string ToString()
        {
            InternalQuery<T> internalQuery = this.GenenateIterator();
            return internalQuery.ToString();
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
            OrderExpression e = new OrderExpression(QueryExpressionType.ThenBy, typeof(T), this.QueryExpression, predicate);
            return new OrderedQuery<T>(this._dbSession, this._dbServiceProvider, e);
        }
        public IOrderedQuery<T> ThenByDesc<K>(Expression<Func<T, K>> predicate)
        {
            OrderExpression e = new OrderExpression(QueryExpressionType.ThenByDesc, typeof(T), this.QueryExpression, predicate);
            return new OrderedQuery<T>(this._dbSession, this._dbServiceProvider, e);
        }
    }
}
