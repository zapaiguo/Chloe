using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe.Core;
using Chloe.Database;
using Chloe.DbProvider;
using Chloe.Query.Implementation;
using Chloe.Query.QueryExpressions;
using Chloe.Utility;

namespace Chloe.Query
{
    internal class Query<T> : IQuery<T>, IEnumerable<T>, IEnumerable
    {
        private QueryExpression _expression;
        protected DatabaseContext _databaseContext;
        protected IDbProvider _dbProvider;

        //public Query(QueryExpression exp, DatabaseContext databaseContext)
        //{
        //    this._expression = exp;
        //    _databaseContext = databaseContext;
        //}

        public Query(QueryExpression exp, DatabaseContext databaseContext, IDbProvider dbProvider)
        {
            this._expression = exp;
            _databaseContext = databaseContext;
            _dbProvider = dbProvider;
        }

        public IQuery<T> Select(Expression<Func<T, object>> selector)
        {
            SelectExpression e = new SelectExpression(_expression, selector);
            return new Query<T>(e, this._databaseContext, this._dbProvider);
        }
        public IQuery<T> Select<T1>(Expression<Func<T, T1>> selector, Expression<Func<T1, object>> selector1)
        {
            SelectTwoExpression e = new SelectTwoExpression(_expression, selector, selector1);
            return new Query<T>(e, this._databaseContext, this._dbProvider);
        }

        public IQuery<T> Where(Expression<Func<T, bool>> predicate)
        {
            WhereExpression e = new WhereExpression(_expression, predicate);
            return new Query<T>(e, this._databaseContext, this._dbProvider);
        }

        public IQuery<T> Skip(int count)
        {
            SkipExpression e = new SkipExpression(_expression, count);
            return new Query<T>(e, this._databaseContext, this._dbProvider);
        }
        public IQuery<T> Take(int count)
        {
            TakeExpression e = new TakeExpression(_expression, count);
            return new Query<T>(e, this._databaseContext, this._dbProvider);
        }

        public IOrderedQuery<T> OrderBy<K>(Expression<Func<T, K>> predicate)
        {
            OrderByExpression e = new OrderByExpression(_expression, predicate);
            return new OrderedQuery<T>(e, this._databaseContext, this._dbProvider);
        }
        public IOrderedQuery<T> OrderByDesc<K>(Expression<Func<T, K>> predicate)
        {
            OrderByDescExpression e = new OrderByDescExpression(_expression, predicate);
            return new OrderedQuery<T>(e, this._databaseContext, this._dbProvider);
        }

        public T QueryObject()
        {
            return ((Query<T>)this.Take(1)).FirstOrDefault();
        }
        public T QueryObject(Expression<Func<T, bool>> predicate)
        {
            return ((Query<T>)(this.Where(predicate).Take(1))).FirstOrDefault();
        }
        public List<T> QueryList()
        {
            return ((Query<T>)(this)).ToList();
        }

        public bool Exists()
        {
            return this.GetExistsExecuteResult();
        }
        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            return ((Query<T>)(this.Where(predicate))).GetExistsExecuteResult();
        }

        public int Count()
        {
            CountExpression e = new CountExpression(_expression);
            var result = this.GetExecuteScalarResult(e);
            return Convert.ToInt32(result);
        }
        public long LongCount()
        {
            LongCountExpression e = new LongCountExpression(_expression);
            var result = this.GetExecuteScalarResult(e);
            return Convert.ToInt64(result);
        }

        public int Sum(Expression<Func<T, int>> selector)
        {
            var result = this.GetSumExecuteScalarResult(selector, Utils.TypeOfInt32);
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }
        public int? Sum(Expression<Func<T, int?>> selector)
        {
            var result = this.GetSumExecuteScalarResult(selector, Utils.TypeOfInt32_Nullable);
            return result == DBNull.Value ? null : new Nullable<Int32>(Convert.ToInt32(result));
        }
        public long Sum(Expression<Func<T, long>> selector)
        {
            var result = this.GetSumExecuteScalarResult(selector, Utils.TypeOfInt64);
            return result == DBNull.Value ? 0 : Convert.ToInt64(result);
        }
        public long? Sum(Expression<Func<T, long?>> selector)
        {
            var result = this.GetSumExecuteScalarResult(selector, Utils.TypeOfInt64_Nullable);
            return result == DBNull.Value ? null : new Nullable<Int64>(Convert.ToInt64(result));
        }
        public decimal Sum(Expression<Func<T, decimal>> selector)
        {
            var result = this.GetSumExecuteScalarResult(selector, Utils.TypeOfDecimal);
            return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
        }
        public decimal? Sum(Expression<Func<T, decimal?>> selector)
        {
            var result = this.GetSumExecuteScalarResult(selector, Utils.TypeOfDecimal_Nullable);
            return result == DBNull.Value ? null : new Nullable<Decimal>(Convert.ToDecimal(result));
        }
        public double Sum(Expression<Func<T, double>> selector)
        {
            var result = this.GetSumExecuteScalarResult(selector, Utils.TypeOfDouble);
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }
        public double? Sum(Expression<Func<T, double?>> selector)
        {
            var result = this.GetSumExecuteScalarResult(selector, Utils.TypeOfDouble_Nullable);
            return result == DBNull.Value ? null : new Nullable<Double>(Convert.ToDouble(result));
        }
        public float Sum(Expression<Func<T, float>> selector)
        {
            var result = this.GetSumExecuteScalarResult(selector, Utils.TypeOfSingle);
            return result == DBNull.Value ? 0 : Convert.ToSingle(result);
        }
        public float? Sum(Expression<Func<T, float?>> selector)
        {
            var result = this.GetSumExecuteScalarResult(selector, Utils.TypeOfSingle_Nullable);
            return result == DBNull.Value ? null : new Nullable<float>(Convert.ToSingle(result));
        }

        public int Max(Expression<Func<T, int>> selector)
        {
            var result = this.GetMaxExecuteScalarResult(selector);
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }
        public int? Max(Expression<Func<T, int?>> selector)
        {
            var result = this.GetMaxExecuteScalarResult(selector);
            return result == DBNull.Value ? null : new Nullable<Int32>(Convert.ToInt32(result));
        }
        public long Max(Expression<Func<T, long>> selector)
        {
            var result = this.GetMaxExecuteScalarResult(selector);
            return result == DBNull.Value ? 0 : Convert.ToInt64(result);
        }
        public long? Max(Expression<Func<T, long?>> selector)
        {
            var result = this.GetMaxExecuteScalarResult(selector);
            return result == DBNull.Value ? null : new Nullable<Int64>(Convert.ToInt64(result));
        }
        public decimal Max(Expression<Func<T, decimal>> selector)
        {
            var result = this.GetMaxExecuteScalarResult(selector);
            return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
        }
        public decimal? Max(Expression<Func<T, decimal?>> selector)
        {
            var result = this.GetMaxExecuteScalarResult(selector);
            return result == DBNull.Value ? null : new Nullable<Decimal>(Convert.ToDecimal(result));
        }
        public double Max(Expression<Func<T, double>> selector)
        {
            var result = this.GetMaxExecuteScalarResult(selector);
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }
        public double? Max(Expression<Func<T, double?>> selector)
        {
            var result = this.GetMaxExecuteScalarResult(selector);
            return result == DBNull.Value ? null : new Nullable<Double>(Convert.ToDouble(result));
        }
        public float Max(Expression<Func<T, float>> selector)
        {
            var result = this.GetMaxExecuteScalarResult(selector);
            return result == DBNull.Value ? 0 : Convert.ToSingle(result);
        }
        public float? Max(Expression<Func<T, float?>> selector)
        {
            var result = this.GetMaxExecuteScalarResult(selector);
            return result == DBNull.Value ? null : new Nullable<float>(Convert.ToSingle(result));
        }

        public int Min(Expression<Func<T, int>> selector)
        {
            var result = this.GetMinExecuteScalarResult(selector);
            return result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }
        public int? Min(Expression<Func<T, int?>> selector)
        {
            var result = this.GetMinExecuteScalarResult(selector);
            return result == DBNull.Value ? null : new Nullable<Int32>(Convert.ToInt32(result));
        }
        public long Min(Expression<Func<T, long>> selector)
        {
            var result = this.GetMinExecuteScalarResult(selector);
            return result == DBNull.Value ? 0 : Convert.ToInt64(result);
        }
        public long? Min(Expression<Func<T, long?>> selector)
        {
            var result = this.GetMinExecuteScalarResult(selector);
            return result == DBNull.Value ? null : new Nullable<Int64>(Convert.ToInt64(result));
        }
        public decimal Min(Expression<Func<T, decimal>> selector)
        {
            var result = this.GetMinExecuteScalarResult(selector);
            return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
        }
        public decimal? Min(Expression<Func<T, decimal?>> selector)
        {
            var result = this.GetMinExecuteScalarResult(selector);
            return result == DBNull.Value ? null : new Nullable<Decimal>(Convert.ToDecimal(result));
        }
        public double Min(Expression<Func<T, double>> selector)
        {
            var result = this.GetMinExecuteScalarResult(selector);
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }
        public double? Min(Expression<Func<T, double?>> selector)
        {
            var result = this.GetMinExecuteScalarResult(selector);
            return result == DBNull.Value ? null : new Nullable<Double>(Convert.ToDouble(result));
        }
        public float Min(Expression<Func<T, float>> selector)
        {
            var result = this.GetMinExecuteScalarResult(selector);
            return result == DBNull.Value ? 0 : Convert.ToSingle(result);
        }
        public float? Min(Expression<Func<T, float?>> selector)
        {
            var result = this.GetMinExecuteScalarResult(selector);
            return result == DBNull.Value ? null : new Nullable<float>(Convert.ToSingle(result));
        }

        public double Average(Expression<Func<T, int>> selector)
        {
            var result = this.GetAverageExecuteScalarResult(selector, Utils.TypeOfDouble);
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }
        public double? Average(Expression<Func<T, int?>> selector)
        {
            var result = this.GetAverageExecuteScalarResult(selector, Utils.TypeOfDouble_Nullable);
            return result == DBNull.Value ? null : new Nullable<Double>(Convert.ToDouble(result));
        }
        public double Average(Expression<Func<T, long>> selector)
        {
            var result = this.GetAverageExecuteScalarResult(selector, Utils.TypeOfDouble);
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }
        public double? Average(Expression<Func<T, long?>> selector)
        {
            var result = this.GetAverageExecuteScalarResult(selector, Utils.TypeOfDouble_Nullable);
            return result == DBNull.Value ? null : new Nullable<Double>(Convert.ToDouble(result));
        }
        public decimal Average(Expression<Func<T, decimal>> selector)
        {
            var result = this.GetAverageExecuteScalarResult(selector, Utils.TypeOfDecimal);
            return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
        }
        public decimal? Average(Expression<Func<T, decimal?>> selector)
        {
            var result = this.GetAverageExecuteScalarResult(selector, Utils.TypeOfDecimal_Nullable);
            return result == DBNull.Value ? null : new Nullable<Decimal>(Convert.ToDecimal(result));
        }
        public double Average(Expression<Func<T, double>> selector)
        {
            var result = this.GetAverageExecuteScalarResult(selector, Utils.TypeOfDouble);
            return result == DBNull.Value ? 0 : Convert.ToDouble(result);
        }
        public double? Average(Expression<Func<T, double?>> selector)
        {
            var result = this.GetAverageExecuteScalarResult(selector, Utils.TypeOfDouble_Nullable);
            return result == DBNull.Value ? null : new Nullable<Double>(Convert.ToDouble(result));
        }
        public float Average(Expression<Func<T, float>> selector)
        {
            var result = this.GetAverageExecuteScalarResult(selector, Utils.TypeOfSingle);
            return result == DBNull.Value ? 0 : Convert.ToSingle(result);
        }
        public float? Average(Expression<Func<T, float?>> selector)
        {
            var result = this.GetAverageExecuteScalarResult(selector, Utils.TypeOfSingle_Nullable);
            return result == DBNull.Value ? null : new Nullable<float>(Convert.ToSingle(result));
        }

        private object GetSumExecuteScalarResult(Expression selector, Type returnType)
        {
            SumExpression e = new SumExpression(_expression, selector, returnType);
            return this.GetExecuteScalarResult(e);
        }
        private object GetMaxExecuteScalarResult(Expression selector)
        {
            MaxExpression e = new MaxExpression(_expression, selector);
            return this.GetExecuteScalarResult(e);
        }
        private object GetMinExecuteScalarResult(Expression selector)
        {
            MinExpression e = new MinExpression(_expression, selector);
            return this.GetExecuteScalarResult(e);
        }
        private object GetAverageExecuteScalarResult(Expression selector, Type returnType)
        {
            AverageExpression e = new AverageExpression(_expression, selector, returnType);
            return this.GetExecuteScalarResult(e);
        }
        private bool GetExistsExecuteResult()
        {
            ExistsExpression e = new ExistsExpression(_expression);
            Dictionary<string, object> parameters;
            string sql = this.TranslateToSql(e, out parameters);
            using (var reader = this._databaseContext.ExecuteInternalReader(CommandType.Text, sql, parameters))
            {
                if (reader.Read())
                {
                    return true;
                }
                return false;
            }
        }

        private object GetExecuteScalarResult(QueryExpression queryExpression)
        {
            Dictionary<string, object> parameters;
            string sql = this.TranslateToSql(queryExpression, out parameters);
            return this._databaseContext.ExecuteScalar(sql, parameters);
        }
        private string TranslateToSql(QueryExpression queryExpression, out Dictionary<string, object> parameters)
        {
            QueryExpressionConverter Converter = QueryExpressionConverter.CreateInstance(this._dbProvider);
            QueryHelper helper = new QueryHelper(this._dbProvider);
            var translate = Converter.Convert(typeof(T), queryExpression);
            string sql = translate.ToSql(helper);
            parameters = helper.Parameters;
            return sql;
        }

        public IEnumerator<T> GetEnumerator()
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            QueryExpressionConverter converter = QueryExpressionConverter.CreateInstance(this._dbProvider);
            QueryHelper helper = new QueryHelper(this._dbProvider);
            ObjectCreateContext createContext = new ObjectCreateContext();
            var translate = converter.Convert(typeof(T), ((IQuery)this).QueryExpression);
            string sql = translate.ToSql(helper, createContext);
            //sw.Stop();
            //Console.WriteLine("构建SQL用时：" + sw.ElapsedMilliseconds);
            var reader = this._databaseContext.ExecuteReader(sql, helper.Parameters, CommandBehavior.SingleResult);
            return new Creator<T>(this._databaseContext, reader, new ObjectCreator<T>(createContext)).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        QueryExpression IQuery.QueryExpression
        {
            get { return _expression; }
        }

        public override string ToString()
        {
            QueryExpressionConverter converter = QueryExpressionConverter.CreateInstance(this._dbProvider);
            QueryHelper helper = new QueryHelper(this._dbProvider);
            ObjectCreateContext createContext = new ObjectCreateContext();
            var translate = converter.Convert(typeof(T), ((IQuery)this).QueryExpression);
            string sql = translate.ToSql(helper, createContext);
            return sql;
        }
    }

    internal class OrderedQuery<T> : Query<T>, IOrderedQuery<T>
    {
        public OrderedQuery(QueryExpression exp, DatabaseContext connectionContext, IDbProvider dbProvider)
            : base(exp, connectionContext, dbProvider)
        {

        }
        public IOrderedQuery<T> ThenBy<K>(Expression<Func<T, K>> predicate)
        {
            ThenByExpression e = new ThenByExpression(((IQuery)this).QueryExpression, predicate);
            return new OrderedQuery<T>(e, _databaseContext, this._dbProvider);
        }
        public IOrderedQuery<T> ThenByDesc<K>(Expression<Func<T, K>> predicate)
        {
            ThenByDescExpression e = new ThenByDescExpression(((IQuery)this).QueryExpression, predicate);
            return new OrderedQuery<T>(e, _databaseContext, this._dbProvider);
        }
    }
}
