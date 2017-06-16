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
        IQuery<TEntity> Query<TEntity>(string table);
        TEntity QueryByKey<TEntity>(object key, bool tracking = false);
        TEntity QueryByKey<TEntity>(object key, string table, bool tracking = false);

        /// <summary>
        /// context.JoinQuery&lt;User, City&gt;((user, city) => new object[] { 
        ///     JoinType.LeftJoin, user.CityId == city.Id 
        /// })
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="joinInfo"></param>
        /// <returns></returns>
        IJoiningQuery<T1, T2> JoinQuery<T1, T2>(Expression<Func<T1, T2, object[]>> joinInfo);
        /// <summary>
        /// context.JoinQuery&lt;User, City, Province&gt;((user, city, province) => new object[] { 
        ///     JoinType.LeftJoin, user.CityId == city.Id, 
        ///     JoinType.LeftJoin, city.ProvinceId == province.Id 
        /// })
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="joinInfo"></param>
        /// <returns></returns>
        IJoiningQuery<T1, T2, T3> JoinQuery<T1, T2, T3>(Expression<Func<T1, T2, T3, object[]>> joinInfo);
        IJoiningQuery<T1, T2, T3, T4> JoinQuery<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, object[]>> joinInfo);
        IJoiningQuery<T1, T2, T3, T4, T5> JoinQuery<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5, object[]>> joinInfo);

        IEnumerable<T> SqlQuery<T>(string sql, params DbParam[] parameters);
        IEnumerable<T> SqlQuery<T>(string sql, CommandType cmdType, params DbParam[] parameters);

        TEntity Insert<TEntity>(TEntity entity);
        TEntity Insert<TEntity>(TEntity entity, string table);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="body"></param>
        /// <returns>It will return null if an entity does not define primary key,other wise return primary key value.</returns>
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
