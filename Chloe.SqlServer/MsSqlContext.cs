using Chloe.Core.Visitors;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe.SqlServer
{
    public class MsSqlContext : DbContext
    {
        string _connString;
        public MsSqlContext(string connString)
            : base(CreateDbServiceProvider(connString))
        {
            this._connString = connString;
        }

        static IDbServiceProvider CreateDbServiceProvider(string connString)
        {
            DbServiceProvider provider = new DbServiceProvider(connString);
            return provider;
        }

        public override T Insert<T>(T entity)
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

                DbExpression valExp = DbExpression.Parameter(val, memberDescriptor.MemberInfoType);
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

            AbstractDbExpressionVisitor dbExpVisitor = this.DbServiceProvider.CreateDbExpressionVisitor();
            var sqlState = e.Accept(dbExpVisitor);

            string sql = sqlState.ToSql();
            sql += ";SELECT @@IDENTITY";

#if DEBUG
            Debug.WriteLine(sql);
#endif

            object retIdentity = this.CurrentSession.ExecuteScalar(sql, dbExpVisitor.ParameterStorage);

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
        public override object Insert<T>(Expression<Func<T>> body)
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

                DbParameterExpression p = DbExpression.Parameter(val, key.MemberInfoType);
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

            AbstractDbExpressionVisitor dbExpVisitor = this.DbServiceProvider.CreateDbExpressionVisitor();
            var sqlState = e.Accept(dbExpVisitor);

            string sql = sqlState.ToSql();
            sql += ";SELECT @@IDENTITY";

#if DEBUG
            Debug.WriteLine(sql);
#endif

            object retIdentity = this.CurrentSession.ExecuteScalar(sql, dbExpVisitor.ParameterStorage);

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

        public override int Update<T>(T entity)
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
                DbExpression valExp = DbExpression.Parameter(val, memberDescriptor.MemberInfoType);
                updateColumns.Add(memberDescriptor, valExp);
            }

            if (keyVal == null)
                throw new Exception(string.Format("实体主键 {0} 值为 null", keyMember.Name));

            if (updateColumns.Count == 0)
                throw new Exception();

            DbExpression left = new DbColumnAccessExpression(typeDescriptor.Table, keyMemberDescriptor.Column);
            DbExpression right = DbExpression.Parameter(keyVal, keyMemberDescriptor.MemberInfoType);
            DbExpression conditionExp = new DbEqualExpression(left, right);

            DbUpdateExpression e = new DbUpdateExpression(typeDescriptor.Table, conditionExp);

            foreach (var item in updateColumns)
            {
                e.UpdateColumns.Add(item.Key.Column, item.Value);
            }

            return this.ExecuteSqlCommand(e);
        }
        public override int Update<T>(Expression<Func<T, T>> body, Expression<Func<T, bool>> condition)
        {
            Utils.CheckNull(body);
            Utils.CheckNull(condition);

            MappingTypeDescriptor typeDescriptor = MappingTypeDescriptor.GetEntityDescriptor(typeof(T));

            Dictionary<MappingMemberDescriptor, DbExpression> updateColumns = typeDescriptor.UpdateBodyExpressionVisitor.Visit(body);
            var conditionExp = typeDescriptor.Visitor.VisitFilterPredicate(condition);

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

        int ExecuteSqlCommand(DbExpression e)
        {
            AbstractDbExpressionVisitor dbExpVisitor = this.DbServiceProvider.CreateDbExpressionVisitor();
            var sqlState = e.Accept(dbExpVisitor);

            string sql = sqlState.ToSql();

#if DEBUG
            Debug.WriteLine(sql);
#endif

            int r = this.CurrentSession.ExecuteNonQuery(sql, dbExpVisitor.ParameterStorage);
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
