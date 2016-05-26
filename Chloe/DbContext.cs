using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using Chloe.Query;
using Chloe.Core;
using Chloe.Utility;
using Chloe.Infrastructure;
using Chloe.Descriptors;
using Chloe.Query.Visitors;
using Chloe.DbExpressions;
using Chloe.Mapper;
using Chloe.Query.Internals;
using Chloe.Core.Visitors;

namespace Chloe
{
    public abstract class DbContext : IDbContext, IDisposable
    {
        bool _disposed = false;
        InternalDbSession _innerDbSession;
        IDbServiceProvider _dbServiceProvider;
        DbSession _currentSession;

        Dictionary<Type, TrackEntityCollection> _trackedEntityContainer;

        Dictionary<Type, TrackEntityCollection> TrackedEntityContainer
        {
            get
            {
                if (this._trackedEntityContainer == null)
                {
                    this._trackedEntityContainer = new Dictionary<Type, TrackEntityCollection>();
                }

                return this._trackedEntityContainer;
            }
        }

        internal InternalDbSession InnerDbSession { get { return this._innerDbSession; } }
        public IDbServiceProvider DbServiceProvider { get { return this._dbServiceProvider; } }

        protected DbContext(IDbServiceProvider dbServiceProvider)
        {
            Utils.CheckNull(dbServiceProvider, "dbServiceProvider");

            this._dbServiceProvider = dbServiceProvider;
            this._innerDbSession = new InternalDbSession(dbServiceProvider.CreateConnection());
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
        public virtual IEnumerable<T> SqlQuery<T>(string sql, IDictionary<string, object> parameters = null) where T : new()
        {
            Utils.CheckNull(sql, "sql");

            return new InternalSqlQuery<T>(this._innerDbSession, sql, parameters);
        }

        public virtual T Insert<T>(T entity)
        {
            Utils.CheckNull(entity);

            TypeDescriptor typeDescriptor = TypeDescriptor.GetDescriptor(entity.GetType());
            EnsureMappingTypeHasPrimaryKey(typeDescriptor);

            MappingMemberDescriptor keyMemberDescriptor = typeDescriptor.PrimaryKey;
            var keyMember = typeDescriptor.PrimaryKey.MemberInfo;

            object keyValue = null;

            Dictionary<MappingMemberDescriptor, DbExpression> insertColumns = new Dictionary<MappingMemberDescriptor, DbExpression>();
            foreach (var kv in typeDescriptor.MappingMemberDescriptors)
            {
                var member = kv.Key;
                var memberDescriptor = kv.Value;

                var val = memberDescriptor.GetValue(entity);

                if (memberDescriptor == keyMemberDescriptor)
                {
                    keyValue = val;
                }

                DbExpression valExp = DbExpression.Parameter(val, memberDescriptor.MemberInfoType);
                insertColumns.Add(memberDescriptor, valExp);
            }

            //主键为空并且主键又不是自增列
            if (keyValue == null)
            {
                throw new Exception(string.Format("主键 {0} 值为 null", keyMemberDescriptor.MemberInfo.Name));
            }

            DbInsertExpression e = new DbInsertExpression(typeDescriptor.Table);

            foreach (var kv in insertColumns)
            {
                e.InsertColumns.Add(kv.Key.Column, kv.Value);
            }

            this.ExecuteSqlCommand(e);
            return entity;
        }
        public virtual object Insert<T>(Expression<Func<T>> body)
        {
            Utils.CheckNull(body);

            TypeDescriptor typeDescriptor = TypeDescriptor.GetDescriptor(typeof(T));
            EnsureMappingTypeHasPrimaryKey(typeDescriptor);

            MappingMemberDescriptor keyMemberDescriptor = typeDescriptor.PrimaryKey;

            Dictionary<MappingMemberDescriptor, object> insertColumns = typeDescriptor.InsertBodyExpressionVisitor.Visit(body);

            DbInsertExpression e = new DbInsertExpression(typeDescriptor.Table);

            object keyValue = null;

            foreach (var kv in insertColumns)
            {
                var key = kv.Key;
                var val = kv.Value;

                if (key.IsPrimaryKey && val == null)
                {
                    throw new Exception(string.Format("主键 {0} 值为 null", keyMemberDescriptor.MemberInfo.Name));
                }
                else
                    keyValue = val;

                DbParameterExpression p = DbExpression.Parameter(val, key.MemberInfoType);
                e.InsertColumns.Add(kv.Key.Column, p);
            }

            //主键为空
            if (keyValue == null)
            {
                throw new Exception(string.Format("主键 {0} 值为 null", keyMemberDescriptor.MemberInfo.Name));
            }

            this.ExecuteSqlCommand(e);
            return keyValue;
        }

        public virtual int Update<T>(T entity)
        {
            Utils.CheckNull(entity);

            TypeDescriptor typeDescriptor = TypeDescriptor.GetDescriptor(entity.GetType());
            EnsureMappingTypeHasPrimaryKey(typeDescriptor);

            object keyVal = null;
            MappingMemberDescriptor keyMemberDescriptor = typeDescriptor.PrimaryKey;
            MemberInfo keyMember = keyMemberDescriptor.MemberInfo;

            IEntityState entityState = this.TryGetTrackedEntityState(entity);
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

                var val = memberDescriptor.GetValue(entity);

                if (entityState != null && !entityState.IsChanged(member, val))
                    continue;

                DbExpression valExp = DbExpression.Parameter(val, memberDescriptor.MemberInfoType);
                updateColumns.Add(memberDescriptor, valExp);
            }

            if (keyVal == null)
                throw new Exception(string.Format("实体主键 {0} 值为 null", keyMember.Name));

            if (updateColumns.Count == 0)
                return 0;

            DbExpression left = new DbColumnAccessExpression(typeDescriptor.Table, keyMemberDescriptor.Column);
            DbExpression right = DbExpression.Parameter(keyVal, keyMemberDescriptor.MemberInfoType);
            DbExpression conditionExp = new DbEqualExpression(left, right);

            DbUpdateExpression e = new DbUpdateExpression(typeDescriptor.Table, conditionExp);

            foreach (var item in updateColumns)
            {
                e.UpdateColumns.Add(item.Key.Column, item.Value);
            }

            int ret = this.ExecuteSqlCommand(e);
            if (entityState != null)
                entityState.Refresh();
            return ret;
        }
        public virtual int Update<T>(Expression<Func<T, T>> body, Expression<Func<T, bool>> condition)
        {
            Utils.CheckNull(body);
            Utils.CheckNull(condition);

            TypeDescriptor typeDescriptor = TypeDescriptor.GetDescriptor(typeof(T));

            Dictionary<MappingMemberDescriptor, DbExpression> updateColumns = typeDescriptor.UpdateBodyExpressionVisitor.Visit(body);
            var conditionExp = typeDescriptor.Visitor.VisitFilterPredicate(condition);

            DbUpdateExpression e = new DbUpdateExpression(typeDescriptor.Table, conditionExp);

            foreach (var item in updateColumns)
            {
                MappingMemberDescriptor key = item.Key;
                if (key.IsPrimaryKey)
                    throw new Exception(string.Format("成员 {0} 属于主键，无法对其进行更新操作", key.MemberInfo.Name));

                e.UpdateColumns.Add(item.Key.Column, item.Value);
            }

            return this.ExecuteSqlCommand(e);
        }

        public virtual int Delete<T>(T entity)
        {
            Utils.CheckNull(entity);

            TypeDescriptor typeDescriptor = TypeDescriptor.GetDescriptor(entity.GetType());
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

            TypeDescriptor typeDescriptor = TypeDescriptor.GetDescriptor(typeof(T));
            var conditionExp = typeDescriptor.Visitor.VisitFilterPredicate(condition);

            DbDeleteExpression e = new DbDeleteExpression(typeDescriptor.Table, conditionExp);

            return this.ExecuteSqlCommand(e);
        }

        public virtual void TrackEntity(object entity)
        {
            Utils.CheckNull(entity);
            Type entityType = entity.GetType();

            if (Utils.IsAnonymousType(entityType))
                return;

            Dictionary<Type, TrackEntityCollection> entityContainer = this.TrackedEntityContainer;

            TrackEntityCollection collection;
            if (!entityContainer.TryGetValue(entityType, out collection))
            {
                TypeDescriptor typeDescriptor = TypeDescriptor.GetDescriptor(entityType);

                if (!typeDescriptor.HasPrimaryKey())
                    return;

                collection = new TrackEntityCollection(typeDescriptor);
                entityContainer.Add(entityType, collection);
            }

            collection.TryAddEntity(entity);
        }
        protected virtual IEntityState TryGetTrackedEntityState(object entity)
        {
            Utils.CheckNull(entity);
            Type entityType = entity.GetType();
            Dictionary<Type, TrackEntityCollection> entityContainer = this._trackedEntityContainer;

            if (entityContainer == null)
                return null;

            TrackEntityCollection collection;
            if (!entityContainer.TryGetValue(entityType, out collection))
            {
                return null;
            }

            var ret = collection.TryGetEntityState(entity);
            return ret;
        }

        public void Dispose()
        {
            if (this._disposed)
                return;

            this._innerDbSession.Dispose();
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

            int r = this._innerDbSession.ExecuteNonQuery(sql, dbExpVisitor.ParameterStorage);
            return r;
        }

        static void EnsureMappingTypeHasPrimaryKey(TypeDescriptor typeDescriptor)
        {
            if (typeDescriptor.PrimaryKey == null)
                throw new Exception(string.Format("实体类 {0} 未定义主键", typeDescriptor.EntityType.FullName));
        }


        class TrackEntityCollection
        {
            public TrackEntityCollection(TypeDescriptor typeDescriptor)
            {
                this.TypeDescriptor = typeDescriptor;
                this.Entities = new Dictionary<object, IEntityState>(1);
            }
            public TypeDescriptor TypeDescriptor { get; private set; }
            public Dictionary<object, IEntityState> Entities { get; private set; }
            public bool TryAddEntity(object entity)
            {
                if (this.Entities.ContainsKey(entity))
                {
                    return false;
                }

                IEntityState entityState = new EntityState(this.TypeDescriptor, entity);
                this.Entities.Add(entity, entityState);

                return true;
            }
            public IEntityState TryGetEntityState(object entity)
            {
                IEntityState ret;
                if (!this.Entities.TryGetValue(entity, out ret))
                    ret = null;

                return ret;
            }
        }
    }
}
