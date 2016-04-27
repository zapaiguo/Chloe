using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Chloe.Database;
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

namespace Chloe
{
    public abstract class DbContext : IDbContext, IDisposable
    {
        bool _disposed = false;
        InternalDbSession _dbSession;
        IDbServiceProvider _dbServiceProvider;

        internal InternalDbSession DbSession { get { return this._dbSession; } }
        internal IDbServiceProvider DbServiceProvider { get { return this._dbServiceProvider; } }

        protected DbContext(IDbServiceProvider dbServiceProvider)
        {
            Utils.CheckNull(dbServiceProvider, "dbServiceProvider");

            this._dbServiceProvider = dbServiceProvider;
            this._dbSession = new InternalDbSession(dbServiceProvider.CreateConnection());
        }

        public virtual IQuery<T> Query<T>() where T : new()
        {
            return new Query<T>(this);
        }

        public virtual T Insert<T>(T entity)
        {
            throw new NotImplementedException();
        }

        public virtual int Update<T>(T entity)
        {
            Utils.CheckNull(entity);

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(typeof(T));

            var keyMember = typeDescriptor.PrimaryKeys.FirstOrDefault();

            if (keyMember == null)
                throw new Exception(string.Format("实体类型 {0} 未定义主键", typeDescriptor.EntityType.FullName));

            object keyVal = null;
            MappingMemberDescriptor keyMemberDescriptor = null;

            Dictionary<DbColumn, DbExpression> updateColumns = new Dictionary<DbColumn, DbExpression>();
            foreach (var kv in typeDescriptor.MappingMemberDescriptors)
            {
                var member = kv.Key;
                var memberDescriptor = kv.Value;

                var val = memberDescriptor.GetValue(entity);
                if (member == keyMember)
                {
                    keyVal = val;
                    keyMemberDescriptor = memberDescriptor;
                    continue;
                }

                DbExpression valExp = new DbParameterExpression(val);
                updateColumns.Add(memberDescriptor.Column, valExp);
            }

            if (keyVal == null)
                throw new Exception(string.Format("实体主键 {0} 值为 null", keyMember.Name));

            if (updateColumns.Count == 0)
                throw new Exception();

            DbExpression left = new DbColumnAccessExpression(typeDescriptor.Table, keyMemberDescriptor.Column);
            DbExpression right = new DbParameterExpression(keyVal);
            DbExpression conditionExp = new DbEqualExpression(left, right);

            DbUpdateExpression e = new DbUpdateExpression(typeDescriptor.Table, conditionExp);

            foreach (var item in updateColumns)
            {
                e.UpdateColumns.Add(item.Key, item.Value);
            }

            return this.ExecuteSqlCommand(e);
        }
        public virtual int Update<T>(Expression<Func<T, T>> body, Expression<Func<T, bool>> condition)
        {
            Utils.CheckNull(body);
            Utils.CheckNull(condition);

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(typeof(T));

            GeneralExpressionVisitor1 vistor = new GeneralExpressionVisitor1(typeDescriptor);
            Dictionary<DbColumn, DbExpression> updateColumns = UpdateColumnExpressionVisitor.VisitExpression(body, typeDescriptor, vistor);
            var conditionExp = vistor.Visit(condition);

            DbUpdateExpression e = new DbUpdateExpression(typeDescriptor.Table, conditionExp);

            foreach (var item in updateColumns)
            {
                e.UpdateColumns.Add(item.Key, item.Value);
            }

            return this.ExecuteSqlCommand(e);
        }

        public virtual int Delete<T>(T entity)
        {
            Utils.CheckNull(entity);

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(typeof(T));

            var keyMember = typeDescriptor.PrimaryKeys.FirstOrDefault();

            if (keyMember == null)
                throw new Exception(string.Format("实体类型 {0} 未定义主键", typeDescriptor.EntityType.FullName));

            MappingMemberDescriptor memberDescriptor = typeDescriptor.MappingMemberDescriptors[keyMember];
            var val = memberDescriptor.GetValue(entity);

            if (val == null)
                throw new Exception(string.Format("实体主键 {0} 值为 null", keyMember.Name));

            DbExpression left = new DbColumnAccessExpression(typeDescriptor.Table, memberDescriptor.Column);
            DbExpression right = new DbParameterExpression(val);
            DbExpression conditionExp = new DbEqualExpression(left, right);

            DbDeleteExpression e = new DbDeleteExpression(typeDescriptor.Table, conditionExp);
            return this.ExecuteSqlCommand(e);
        }
        public virtual int Delete<T>(Expression<Func<T, bool>> condition)
        {
            Utils.CheckNull(condition);

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(typeof(T));
            GeneralExpressionVisitor1 vistor = new GeneralExpressionVisitor1(typeDescriptor);
            var conditionExp = vistor.Visit(condition);

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
            DbExpressionVisitorBase dbExpVisitor = this._dbServiceProvider.CreateDbExpressionVisitor();
            var sqlState = e.Accept(dbExpVisitor);

            string sql = sqlState.ToSql();

#if DEBUG
            Debug.WriteLine(sql);
#endif

            int r = this._dbSession.ExecuteNonQuery(sql, dbExpVisitor.ParameterStorage);
            return r;
        }
    }
}
