using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Chloe.Core
{
    public interface IDbContext : IDisposable
    {
        IQuery<T> Query<T>() where T : new();

        T Insert<T>(T entity);

        int Update<T>(T entity);
        //int Update<T>(Expression<Func<T, bool>> predicate, object obj);

        int Delete<T>(T entity);
        int Delete<T>(Expression<Func<T, bool>> condition);

    }
}
