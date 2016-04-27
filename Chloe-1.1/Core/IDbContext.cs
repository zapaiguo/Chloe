using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Chloe.Core
{
    public interface IDbContext : IDisposable
    {
        //string ConnectionString { get; }
        //bool IsInTransaction { get; }

        IQuery<T> Query<T>() where T : new();

        T Insert<T>(T entity);

        int Update<T>(T entity);
        //int Update<T>(Expression<Func<T, bool>> predicate, object obj);

        int Delete<T>(T entity);
        int Delete<T>(Expression<Func<T, bool>> condition);

        //int ExecuteNonQuery(string sql, IDictionary<string, object> parameters);
        //object ExecuteScalar(string sql, IDictionary<string, object> parameters);
        //IDataReader ExecuteReader(string sql, IDictionary<string, object> parameters);
        //DataTable ExecuteDataTable(string sql, IDictionary<string, object> parameters);

        //IDbConnection CreateConnection();

        //void BeginTran();
        //void BeginTran(IsolationLevel il);
        //void CommitTran();
        //void RollbackTran();
    }
}
