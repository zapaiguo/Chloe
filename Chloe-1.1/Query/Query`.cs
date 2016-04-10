using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe.Core;
using Chloe.Query.Implementation;
using Chloe.Query.QueryExpressions;
using Chloe.Utility;

namespace Chloe.Query
{
    public class Query<T> : IQuery<T>//, IEnumerable<T>, IEnumerable
    {
        QueryExpression _expression;


        //public Query(QueryExpression exp, DatabaseContext databaseContext)
        //{
        //    this._expression = exp;
        //    _databaseContext = databaseContext;
        //}
        public Query()
            : this(new RootQueryExpression(typeof(T)))
        {

        }
        protected Query(QueryExpression exp)
        {
            this._expression = exp;
            //_databaseContext = databaseContext;
            //_dbProvider = dbProvider;
        }

        public IQuery<T1> Select<T1>(Expression<Func<T, T1>> selector)
        {
            SelectExpression e = new SelectExpression(_expression, typeof(T1), selector);
            return new Query<T1>(e);
        }

        //public IQuery<T> Include<TProperty>(Expression<Func<T, TProperty>> path)
        //{
        //    IncludeExpression e = new IncludeExpression(_expression, typeof(T), path);
        //    return new Query<T>(e);
        //}

        public IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            WhereExpression e = new WhereExpression(_expression, typeof(T), predicate);
            return new Query<T>(e);
        }

        public IQuery<T> Skip(int count)
        {
            SkipExpression e = new SkipExpression(_expression, typeof(T), count);
            return new Query<T>(e);
        }
        public IQuery<T> Take(int count)
        {
            TakeExpression e = new TakeExpression(_expression, typeof(T), count);
            return new Query<T>(e);
        }

        public IOrderedQuery<T> OrderBy<K>(Expression<Func<T, K>> predicate)
        {
            OrderExpression e = new OrderExpression(QueryExpressionType.OrderBy, typeof(T), ((IQuery)this).QueryExpression, predicate);
            return new OrderedQuery<T>(e);
        }
        public IOrderedQuery<T> OrderByDesc<K>(Expression<Func<T, K>> predicate)
        {
            OrderExpression e = new OrderExpression(QueryExpressionType.OrderByDesc, typeof(T), ((IQuery)this).QueryExpression, predicate);
            return new OrderedQuery<T>(e);
        }

        public T QueryObject()
        {
            throw new NotImplementedException();
        }
        public T QueryObject(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }
        public List<T> QueryList()
        {
            throw new NotImplementedException();
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

    }

    internal class OrderedQuery<T> : Query<T>, IOrderedQuery<T>
    {
        public OrderedQuery(QueryExpression exp)
            : base(exp)
        {

        }
        public IOrderedQuery<T> ThenBy<K>(Expression<Func<T, K>> predicate)
        {
            OrderExpression e = new OrderExpression(QueryExpressionType.ThenBy, typeof(T), ((IQuery)this).QueryExpression, predicate);
            return new OrderedQuery<T>(e);
        }
        public IOrderedQuery<T> ThenByDesc<K>(Expression<Func<T, K>> predicate)
        {
            OrderExpression e = new OrderExpression(QueryExpressionType.ThenByDesc, typeof(T), ((IQuery)this).QueryExpression, predicate);
            return new OrderedQuery<T>(e);
        }
    }
}
