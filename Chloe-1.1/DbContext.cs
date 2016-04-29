using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Chloe.Query;
using Chloe.Core;
using Chloe.Utility;
using Chloe.Infrastructure;
using Chloe.Descriptors;
using Chloe.Query.Visitors;
using Chloe.DbExpressions;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Chloe.Impls;
using Chloe.Mapper;
using Chloe.Query.Internals;
using Chloe.Core.Visitors;

namespace Chloe
{
    public abstract class DbContext : IDbContext, IDisposable
    {
        bool _disposed = false;
        InternalDbSession _dbSession;
        IDbServiceProvider _dbServiceProvider;

        DbSession _currentSession;

        internal InternalDbSession DbSession { get { return this._dbSession; } }
        public IDbServiceProvider DbServiceProvider { get { return this._dbServiceProvider; } }

        protected DbContext(IDbServiceProvider dbServiceProvider)
        {
            Utils.CheckNull(dbServiceProvider, "dbServiceProvider");

            this._dbServiceProvider = dbServiceProvider;
            this._dbSession = new InternalDbSession(dbServiceProvider.CreateConnection());
            this._currentSession = new DbSession(this);
        }

        public IDbSession CurrentSession
        {
            get
            {
                return this._currentSession;
            }
        }

        public virtual IQuery<T> Query<T>() where T : new()
        {
            return new Query<T>(this);
        }
        public virtual IEnumerable<T> SqlQuery<T>(string sql, IDictionary<string, object> parameters) where T : new()
        {
            Utils.CheckNull(sql, "sql");

            return new InternalSqlQuery<T>(this._dbSession, sql, parameters);
        }

        public virtual T Insert<T>(T entity)
        {
            Utils.CheckNull(entity);

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(entity.GetType());
            EnsureMappingTypeHasPrimaryKey(typeDescriptor);

            MappingMemberDescriptor keyMemberDescriptor = typeDescriptor.PrimaryKey;
            var keyMember = typeDescriptor.PrimaryKey.MemberInfo;

            object keyValue = null;

            MappingMemberDescriptor autoIncrementMemberDescriptor = GetAutoIncrementMemberDescriptor(typeDescriptor);

            Dictionary<MappingMemberDescriptor, DbExpression> insertColumns = new Dictionary<MappingMemberDescriptor, DbExpression>();
            foreach (var kv in typeDescriptor.MappingMemberDescriptors)
            {
                var member = kv.Key;
                var memberDescriptor = kv.Value;

                if (memberDescriptor == autoIncrementMemberDescriptor)
                    continue;

                var val = memberDescriptor.GetValue(entity);

                if (memberDescriptor == keyMemberDescriptor)
                {
                    keyValue = val;
                }

                DbExpression valExp = new DbParameterExpression(val ?? DBNull.Value);
                insertColumns.Add(memberDescriptor, valExp);
            }

            //主键为空并且主键又不是自增列
            if (keyValue == null && keyMemberDescriptor != autoIncrementMemberDescriptor)
            {
                throw new Exception(string.Format("主键 {0} 值为 null", keyMemberDescriptor.MemberInfo.Name));
            }

            DbInsertExpression e = new DbInsertExpression(typeDescriptor.Table);

            foreach (var kv in insertColumns)
            {
                e.InsertColumns.Add(kv.Key.Column, kv.Value);
            }

            if (autoIncrementMemberDescriptor == null)
            {
                this.ExecuteSqlCommand(e);
                return entity;
            }

            AbstractDbExpressionVisitor dbExpVisitor = this._dbServiceProvider.CreateDbExpressionVisitor();
            var sqlState = e.Accept(dbExpVisitor);

            string sql = sqlState.ToSql();
            sql += ";SELECT @@IDENTITY";

#if DEBUG
            Debug.WriteLine(sql);
#endif

            object retIdentity = this._dbSession.ExecuteScalar(sql, dbExpVisitor.ParameterStorage);

            if (retIdentity == null || retIdentity == DBNull.Value)
            {
                throw new Exception("无法获取自增标识");
            }

            //SELECT @@IDENTITY 返回的是 decimal 类型
            decimal identity = (decimal)retIdentity;
            if (autoIncrementMemberDescriptor.MemberInfoType == typeof(int))
            {
                autoIncrementMemberDescriptor.SetValue(entity, (int)identity);
            }
            else if (autoIncrementMemberDescriptor.MemberInfoType == typeof(long))
            {
                autoIncrementMemberDescriptor.SetValue(entity, (long)identity);
            }
            else
                autoIncrementMemberDescriptor.SetValue(entity, retIdentity);

            return entity;
        }
        public virtual object Insert<T>(Expression<Func<T>> body)
        {
            Utils.CheckNull(body);

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(typeof(T));
            EnsureMappingTypeHasPrimaryKey(typeDescriptor);

            MappingMemberDescriptor keyMemberDescriptor = typeDescriptor.PrimaryKey;
            MappingMemberDescriptor autoIncrementMemberDescriptor = GetAutoIncrementMemberDescriptor(typeDescriptor);

            Dictionary<MappingMemberDescriptor, object> insertColumns = typeDescriptor.InsertBodyExpressionVisitor.Visit(body);

            DbInsertExpression e = new DbInsertExpression(typeDescriptor.Table);

            object keyValue = null;

            foreach (var kv in insertColumns)
            {
                var key = kv.Key;
                var val = kv.Value;

                if (key == autoIncrementMemberDescriptor)
                    throw new Exception(string.Format("无法将值插入自增列 {0}", key.Column.Name));

                if (key.IsPrimaryKey && val == null)
                {
                    throw new Exception(string.Format("主键 {0} 值为 null", keyMemberDescriptor.MemberInfo.Name));
                }
                else
                    keyValue = val;

                DbParameterExpression p = new DbParameterExpression(val ?? DBNull.Value);
                e.InsertColumns.Add(kv.Key.Column, p);
            }

            //主键为空并且主键又不是自增列
            if (keyValue == null && keyMemberDescriptor != autoIncrementMemberDescriptor)
            {
                throw new Exception(string.Format("主键 {0} 值为 null", keyMemberDescriptor.MemberInfo.Name));
            }

            if (autoIncrementMemberDescriptor == null)
            {
                this.ExecuteSqlCommand(e);
                return keyValue;
            }

            AbstractDbExpressionVisitor dbExpVisitor = this._dbServiceProvider.CreateDbExpressionVisitor();
            var sqlState = e.Accept(dbExpVisitor);

            string sql = sqlState.ToSql();
            sql += ";SELECT @@IDENTITY";

#if DEBUG
            Debug.WriteLine(sql);
#endif

            object retIdentity = this._dbSession.ExecuteScalar(sql, dbExpVisitor.ParameterStorage);

            if (retIdentity == null || retIdentity == DBNull.Value)
            {
                throw new Exception("无法获取自增标识");
            }

            //SELECT @@IDENTITY 返回的是 decimal 类型
            decimal identity = (decimal)retIdentity;
            if (autoIncrementMemberDescriptor.MemberInfoType == typeof(int))
            {
                return (int)identity;
            }
            else if (autoIncrementMemberDescriptor.MemberInfoType == typeof(long))
            {
                return (long)identity;
            }

            return retIdentity;
        }

        public virtual int Update<T>(T entity)
        {
            Utils.CheckNull(entity);

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(entity.GetType());
            EnsureMappingTypeHasPrimaryKey(typeDescriptor);

            object keyVal = null;
            MappingMemberDescriptor keyMemberDescriptor = typeDescriptor.PrimaryKey;
            MemberInfo keyMember = keyMemberDescriptor.MemberInfo;

            Dictionary<MappingMemberDescriptor, DbExpression> updateColumns = new Dictionary<MappingMemberDescriptor, DbExpression>();
            foreach (var kv in typeDescriptor.MappingMemberDescriptors)
            {
                var member = kv.Key;
                var memberDescriptor = kv.Value;

                if (member == keyMember)
                {
                    keyVal = memberDescriptor.GetValue(entity);
                    keyMemberDescriptor = memberDescriptor;
                    continue;
                }

                AutoIncrementAttribute attr = (AutoIncrementAttribute)memberDescriptor.GetCustomAttribute(typeof(AutoIncrementAttribute));
                if (attr != null)
                    continue;

                var val = memberDescriptor.GetValue(entity);
                DbExpression valExp = new DbParameterExpression(val ?? DBNull.Value);
                updateColumns.Add(memberDescriptor, valExp);
            }

            if (keyVal == null)
                throw new Exception(string.Format("实体主键 {0} 值为 null", keyMember.Name));

            if (updateColumns.Count == 0)
                throw new Exception();

            DbExpression left = new DbColumnAccessExpression(typeDescriptor.Table, keyMemberDescriptor.Column);
            DbExpression right = new DbParameterExpression(keyVal ?? DBNull.Value);
            DbExpression conditionExp = new DbEqualExpression(left, right);

            DbUpdateExpression e = new DbUpdateExpression(typeDescriptor.Table, conditionExp);

            foreach (var item in updateColumns)
            {
                e.UpdateColumns.Add(item.Key.Column, item.Value);
            }

            return this.ExecuteSqlCommand(e);
        }
        public virtual int Update<T>(Expression<Func<T, T>> body, Expression<Func<T, bool>> condition)
        {
            Utils.CheckNull(body);
            Utils.CheckNull(condition);

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(typeof(T));

            Dictionary<MappingMemberDescriptor, DbExpression> updateColumns = typeDescriptor.UpdateBodyExpressionVisitor.Visit(body);
            var conditionExp = typeDescriptor.Visitor.Visit(condition);

            DbUpdateExpression e = new DbUpdateExpression(typeDescriptor.Table, conditionExp);

            foreach (var item in updateColumns)
            {
                MappingMemberDescriptor key = item.Key;
                if (key.IsPrimaryKey)
                    throw new Exception(string.Format("成员 {0} 属于主键，无法对其进行更新操作", key.MemberInfo.Name));

                e.UpdateColumns.Add(item.Key.Column, item.Value);

                AutoIncrementAttribute attr = (AutoIncrementAttribute)key.GetCustomAttribute(typeof(AutoIncrementAttribute));
                if (attr != null)
                    throw new Exception(string.Format("成员 {0} 属于自增成员，无法对其进行更新操作", key.MemberInfo.Name));
            }

            return this.ExecuteSqlCommand(e);
        }

        public virtual int Delete<T>(T entity)
        {
            Utils.CheckNull(entity);

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(entity.GetType());
            EnsureMappingTypeHasPrimaryKey(typeDescriptor);

            MappingMemberDescriptor keyMemberDescriptor = typeDescriptor.PrimaryKey;
            var keyMember = typeDescriptor.PrimaryKey.MemberInfo;

            var val = keyMemberDescriptor.GetValue(entity);

            if (val == null)
                throw new Exception(string.Format("实体主键 {0} 值为 null", keyMember.Name));

            DbExpression left = new DbColumnAccessExpression(typeDescriptor.Table, keyMemberDescriptor.Column);
            DbExpression right = new DbParameterExpression(val);
            DbExpression conditionExp = new DbEqualExpression(left, right);

            DbDeleteExpression e = new DbDeleteExpression(typeDescriptor.Table, conditionExp);
            return this.ExecuteSqlCommand(e);
        }
        public virtual int Delete<T>(Expression<Func<T, bool>> condition)
        {
            Utils.CheckNull(condition);

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(typeof(T));
            var conditionExp = typeDescriptor.Visitor.Visit(condition);

            DbDeleteExpression e = new DbDeleteExpression(typeDescriptor.Table, conditionExp);

            return this.ExecuteSqlCommand(e);
        }

        public void Dispose()
        {
            this._dbSession.Dispose();
            this.Dispose(true);
            this._disposed = true;
        }
        protected virtual void Dispose(bool disposing)
        {

        }

        int ExecuteSqlCommand(DbExpression e)
        {
            AbstractDbExpressionVisitor dbExpVisitor = this._dbServiceProvider.CreateDbExpressionVisitor();
            var sqlState = e.Accept(dbExpVisitor);

            string sql = sqlState.ToSql();

#if DEBUG
            Debug.WriteLine(sql);
#endif

            int r = this._dbSession.ExecuteNonQuery(sql, dbExpVisitor.ParameterStorage);
            return r;
        }

        static void EnsureMappingTypeHasPrimaryKey(MappingTypeDescriptor typeDescriptor)
        {
            if (typeDescriptor.PrimaryKey == null)
                throw new Exception(string.Format("实体类型 {0} 未定义主键", typeDescriptor.EntityType.FullName));
        }

        static MappingMemberDescriptor GetAutoIncrementMemberDescriptor(MappingTypeDescriptor typeDescriptor)
        {
            var autoIncrementMemberDescriptors = typeDescriptor.MappingMemberDescriptors.Values.Where(a =>
            {
                AutoIncrementAttribute attr = (AutoIncrementAttribute)a.GetCustomAttribute(typeof(AutoIncrementAttribute));
                return attr != null;
            }).ToList();

            if (autoIncrementMemberDescriptors.Count > 1)
                throw new Exception(string.Format("实体类型 {0} 定义多个自增成员", typeDescriptor.EntityType.FullName));

            var autoIncrementMemberDescriptor = autoIncrementMemberDescriptors.FirstOrDefault();

            if (autoIncrementMemberDescriptor != null)
                EnsureAutoIncrementMemberType(autoIncrementMemberDescriptor);

            return autoIncrementMemberDescriptor;
        }
        static void EnsureAutoIncrementMemberType(MappingMemberDescriptor autoIncrementMemberDescriptor)
        {
            if (autoIncrementMemberDescriptor.MemberInfoType != typeof(int) && autoIncrementMemberDescriptor.MemberInfoType != typeof(long))
            {
                throw new Exception("自增成员必须是 Int32 或 Int64 类型");
            }
        }

    }
}
