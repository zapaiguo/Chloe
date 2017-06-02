using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Chloe
{
    public interface IDbContext : IDisposable
    {
        IDbSession Session { get; }

        IQuery<TEntity> Query<TEntity>();
        TEntity QueryByKey<TEntity>(object key, bool tracking = false);

        IEnumerable<T> SqlQuery<T>(string sql, params DbParam[] parameters);
        IEnumerable<T> SqlQuery<T>(string sql, CommandType cmdType, params DbParam[] parameters);

        TEntity Insert<TEntity>(TEntity entity);
        TEntity Insert<TEntity>(TEntity entity, string table);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="body"></param>
        /// <returns>PrimaryKey</returns>
        object Insert<TEntity>(Expression<Func<TEntity>> body);
        object Insert<TEntity>(Expression<Func<TEntity>> body, string table);

        int Update<TEntity>(TEntity entity);
        int Update<TEntity>(TEntity entity, string table);
        int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> body);
        int Update<TEntity>(Expression<Func<TEntity, bool>> condition, Expression<Func<TEntity, TEntity>> body, string table);

        int Delete<TEntity>(TEntity entity);
        int Delete<TEntity>(TEntity entity, string table);
        int Delete<TEntity>(Expression<Func<TEntity, bool>> condition);
        int Delete<TEntity>(Expression<Func<TEntity, bool>> condition, string table);
        int DeleteByKey<TEntity>(object key);
        int DeleteByKey<TEntity>(object key, string table);

        void TrackEntity(object entity);
    }
}
