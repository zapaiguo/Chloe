using Chloe.Core;
using Chloe.Core.Visitors;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using System;
using System.Collections.Generic;
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
            : base(CreateDbContextServiceProvider(connString))
        {
            this._connString = connString;
        }

        static IDbContextServiceProvider CreateDbContextServiceProvider(string connString)
        {
            DbContextServiceProvider provider = new DbContextServiceProvider(connString);
            return provider;
        }

        public override T Insert<T>(T entity)
        {
            Utils.CheckNull(entity);

            TypeDescriptor typeDescriptor = TypeDescriptor.GetDescriptor(entity.GetType());
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

            AbstractDbExpressionVisitor dbExpVisitor = this.DbContextServiceProvider.CreateDbExpressionVisitor();
            var sqlState = e.Accept(dbExpVisitor);

            string sql = sqlState.ToSql();
            sql += ";SELECT @@IDENTITY";

            //SELECT @@IDENTITY 返回的是 decimal 类型
            object retIdentity = this.CurrentSession.ExecuteScalar(sql, dbExpVisitor.ParameterStorage);

            if (retIdentity == null || retIdentity == DBNull.Value)
            {
                throw new Exception("无法获取自增标识");
            }

            retIdentity = ConvertIdentity(retIdentity, autoIncrementMemberDescriptor.MemberInfoType);
            autoIncrementMemberDescriptor.SetValue(entity, retIdentity);
            return entity;
        }
        public override object Insert<T>(Expression<Func<T>> body)
        {
            Utils.CheckNull(body);

            TypeDescriptor typeDescriptor = TypeDescriptor.GetDescriptor(typeof(T));
            EnsureMappingTypeHasPrimaryKey(typeDescriptor);

            MappingMemberDescriptor keyMemberDescriptor = typeDescriptor.PrimaryKey;
            MappingMemberDescriptor autoIncrementMemberDescriptor = GetAutoIncrementMemberDescriptor(typeDescriptor);

            Dictionary<MemberInfo, Expression> insertColumns = InitMemberExtractor.Extract(body);

            DbInsertExpression e = new DbInsertExpression(typeDescriptor.Table);

            object keyValue = null;

            foreach (var kv in insertColumns)
            {
                MemberInfo key = kv.Key;
                MappingMemberDescriptor memberDescriptor = typeDescriptor.TryGetMappingMemberDescriptor(key);

                if (memberDescriptor == null)
                    throw new Exception(string.Format("成员 {0} 未映射任何列", key.Name));

                if (memberDescriptor == autoIncrementMemberDescriptor)
                    throw new Exception(string.Format("不能将值插入自增长列 '{0}'", memberDescriptor.Column.Name));

                if (memberDescriptor.IsPrimaryKey)
                {
                    object val = ExpressionEvaluator.Evaluate(kv.Value);
                    if (val == null)
                        throw new Exception(string.Format("主键 {0} 值为 null", memberDescriptor.MemberInfo.Name));
                    else
                    {
                        keyValue = val;
                        e.InsertColumns.Add(memberDescriptor.Column, DbExpression.Parameter(keyValue));
                        continue;
                    }
                }

                e.InsertColumns.Add(memberDescriptor.Column, typeDescriptor.Visitor.Visit(kv.Value));
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

            AbstractDbExpressionVisitor dbExpVisitor = this.DbContextServiceProvider.CreateDbExpressionVisitor();
            var sqlState = e.Accept(dbExpVisitor);

            string sql = sqlState.ToSql();
            sql += ";SELECT @@IDENTITY";

            //SELECT @@IDENTITY 返回的是 decimal 类型
            object retIdentity = this.CurrentSession.ExecuteScalar(sql, dbExpVisitor.ParameterStorage);

            if (retIdentity == null || retIdentity == DBNull.Value)
            {
                throw new Exception("无法获取自增标识");
            }

            retIdentity = ConvertIdentity(retIdentity, autoIncrementMemberDescriptor.MemberInfoType);
            return retIdentity;
        }

        public override int Update<T>(T entity)
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

                AutoIncrementAttribute attr = (AutoIncrementAttribute)memberDescriptor.GetCustomAttribute(typeof(AutoIncrementAttribute));
                if (attr != null)
                    continue;

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
        public override int Update<T>(Expression<Func<T, T>> body, Expression<Func<T, bool>> condition)
        {
            Utils.CheckNull(body);
            Utils.CheckNull(condition);

            TypeDescriptor typeDescriptor = TypeDescriptor.GetDescriptor(typeof(T));

            Dictionary<MemberInfo, Expression> updateColumns = InitMemberExtractor.Extract(body);
            var conditionExp = typeDescriptor.Visitor.VisitFilterPredicate(condition);

            DbUpdateExpression e = new DbUpdateExpression(typeDescriptor.Table, conditionExp);

            foreach (var kv in updateColumns)
            {
                MemberInfo key = kv.Key;
                MappingMemberDescriptor memberDescriptor = typeDescriptor.TryGetMappingMemberDescriptor(key);

                if (memberDescriptor == null)
                    throw new Exception(string.Format("成员 {0} 未映射任何列", key.Name));

                if (memberDescriptor.IsPrimaryKey)
                    throw new Exception(string.Format("无法对主键 '{0}' 进行更新", memberDescriptor.Column.Name));

                AutoIncrementAttribute attr = (AutoIncrementAttribute)key.GetCustomAttribute(typeof(AutoIncrementAttribute));
                if (attr != null)
                    throw new Exception(string.Format("无法对自增长列 '{0}' 进行更新", memberDescriptor.Column.Name));

                e.UpdateColumns.Add(memberDescriptor.Column, typeDescriptor.Visitor.Visit(kv.Value));
            }

            return this.ExecuteSqlCommand(e);
        }

        int ExecuteSqlCommand(DbExpression e)
        {
            AbstractDbExpressionVisitor dbExpVisitor = this.DbContextServiceProvider.CreateDbExpressionVisitor();
            var sqlState = e.Accept(dbExpVisitor);

            string sql = sqlState.ToSql();

            int r = this.CurrentSession.ExecuteNonQuery(sql, dbExpVisitor.ParameterStorage);
            return r;
        }

        static void EnsureMappingTypeHasPrimaryKey(TypeDescriptor typeDescriptor)
        {
            if (!typeDescriptor.HasPrimaryKey())
                throw new Exception(string.Format("实体类 {0} 未定义主键", typeDescriptor.EntityType.FullName));
        }

        static MappingMemberDescriptor GetAutoIncrementMemberDescriptor(TypeDescriptor typeDescriptor)
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
            if (autoIncrementMemberDescriptor.MemberInfoType != UtilConstants.TypeOfInt16 && autoIncrementMemberDescriptor.MemberInfoType != UtilConstants.TypeOfInt32 && autoIncrementMemberDescriptor.MemberInfoType != UtilConstants.TypeOfInt64)
            {
                throw new Exception("自增成员必须是 Int16、Int32 或 Int64 类型");
            }
        }
        static object ConvertIdentity(object identity, Type conversionType)
        {
            if (identity.GetType() != conversionType)
                return Convert.ChangeType(identity, conversionType);

            return identity;
        }

    }
}
