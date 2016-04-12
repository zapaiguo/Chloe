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
using Chloe.Query.Implementation;
using Chloe.Query.QueryExpressions;
using Chloe.Utility;

namespace Chloe.Query
{
    internal class Query<T> : IEnumerable<T>, IEnumerable
    {
        QueryExpression _expression;
        InternalDbSession _dbSession;

        public Query(QueryExpression exp, InternalDbSession dbSession)
        {
            this._expression = exp;
            _dbSession = dbSession;
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
}
