using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Chloe.Core
{
    public interface IDbContext : IDisposable
    {
        IDbSession CurrentSession { get; }
        IQuery<T> Query<T>() where T : new();
        IEnumerable<T> SqlQuery<T>(string sql, IDictionary<string, object> parameters) where T : new();

        T Insert<T>(T entity);
        object Insert<T>(Expression<Func<T>> body);

        int Update<T>(T entity);
        int Update<T>(Expression<Func<T, T>> body, Expression<Func<T, bool>> condition);

        int Delete<T>(T entity);
        int Delete<T>(Expression<Func<T, bool>> condition);

    }
}
