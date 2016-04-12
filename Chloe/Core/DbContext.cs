using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Chloe.Database;
using Chloe.Query;
using Chloe.Extensions;
using Chloe.Core;
using Chloe.Query.Implementation;
using Chloe.Utility;
using Chloe.DbProvider;

namespace Chloe
{
    public abstract class DbContext : IDbContext, IDisposable
    {
        private IDbConnection _connection;
        private DatabaseContext _databaseContext;
        private IDbProvider _dbProvider;

        protected DbContext(IDbProvider dbProvider)
        {
            Utils.CheckNull(dbProvider, "dbProvider");
            _dbProvider = dbProvider;
        }

        protected DbContext(IDbConnection connection, IDbProvider dbProvider)
        {
            Utils.CheckNull(dbProvider, "dbProvider");

            _connection = connection;
            _dbProvider = dbProvider;
        }

        protected DatabaseContext DatabaseContext
        {
            get
            {
                if (_databaseContext == null)
                {
                    if (_connection == null)
                        _connection = this.CreateConnection();
                    _databaseContext = new DatabaseContext(_connection);
                }
                return _databaseContext;
            }

            set { _databaseContext = value; }
        }

        public virtual string ConnectionString { get { return this.DatabaseContext.DbConnection.ConnectionString; } }
        public virtual bool IsInTransaction { get { return this.DatabaseContext.IsInTransaction; } }

        public virtual IQuery<T> Query<T>() where T : new()
        {
            return new Query<T>(null, DatabaseContext, this._dbProvider);
        }

        public virtual T Insert<T>(T entity)
        {
            Utils.CheckNull(entity, "entity");

            var type = entity.GetType();
            var values = DelegateCreateManage.GetGetValuesDelegate(type)(entity);
            EntityDescriptor entityDescriptor = EntityDescriptor.GetEntityDescriptor(type);

            IList<KeyValuePair<string, object>> entityVals = new List<KeyValuePair<string, object>>(entityDescriptor.MapMembers.Count);

            foreach (var item in entityDescriptor.MapMembers)
            {
                if (entityDescriptor.AutoIncrementMember != null && item.Key == entityDescriptor.AutoIncrementMember.MemberInfo)
                    continue;
                foreach (var kv in values)
                {
                    if (item.Key.Name == kv.Key)
                    {
                        if (entityVals.Any(a => a.Key == item.Value.ColumnName))
                            throw new Exception("存在多个 Map 列名相同的成员: " + item.Value.ColumnName);
                        entityVals.Add(new KeyValuePair<string, object>(item.Value.ColumnName, kv.Value));
                        break;
                    }
                }
            }

            IDictionary<string, object> parameters = new Dictionary<string, object>(entityVals.Count); ;

            if (entityDescriptor.AutoIncrementMember != null)
            {
                string sql = this._dbProvider.GetSqlBuilder().BuildSql_Insert(entityDescriptor.TableName, entityVals, ref parameters, true);
                var result = this.DatabaseContext.ExecuteScalar(sql, parameters);
                if (result != DBNull.Value && result != null)
                {
                    result = Convert.ChangeType(result, entityDescriptor.AutoIncrementMember.MemberInfo.GetPropertyOrFieldType());
                    entityDescriptor.AutoIncrementMember.MemberInfo.SetPropertyOrFieldValue(entity, result);
                }
            }
            else
            {
                string sql = this._dbProvider.GetSqlBuilder().BuildSql_Insert(entityDescriptor.TableName, entityVals, ref parameters, false);
                this.DatabaseContext.ExecuteNonQuery(sql, parameters);
            }

            return entity;
        }

        public virtual int Update<T>(T entity)
        {
            Utils.CheckNull(entity, "entity");

            var type = entity.GetType();
            var values = DelegateCreateManage.GetGetValuesDelegate(type)(entity);
            EntityDescriptor entityDescriptor = EntityDescriptor.GetEntityDescriptor(type);

            IList<KeyValuePair<string, object>> entityVals = new List<KeyValuePair<string, object>>(entityDescriptor.MapMembers.Count);

            foreach (var item in entityDescriptor.MapMembers)
            {
                //此方法不更新自增列和主键列
                if ((entityDescriptor.AutoIncrementMember != null && item.Key == entityDescriptor.AutoIncrementMember.MemberInfo) || entityDescriptor.PrimaryKeyMembers.Any(a => a.MemberInfo == item.Key))
                    continue;

                foreach (var kv in values)
                {
                    if (item.Key.Name == kv.Key)
                    {
                        if (entityVals.Any(a => a.Key == item.Value.ColumnName))
                            throw new Exception("存在多个 Map 列名相同的成员: " + item.Value.ColumnName);
                        entityVals.Add(new KeyValuePair<string, object>(item.Value.ColumnName, kv.Value));
                        break;
                    }
                }
            }

            List<KeyValuePair<string, object>> pkValues = this.GetEntityPKValues(entity, entityDescriptor);

            IDictionary<string, object> parameters = new Dictionary<string, object>(entityVals.Count + pkValues.Count);
            string sql = this._dbProvider.GetSqlBuilder().BuildSql_Update(entityDescriptor.TableName, entityVals, pkValues, ref parameters);

            return this.DatabaseContext.ExecuteNonQuery(sql, parameters);
        }
        public virtual int Update<T>(Expression<Func<T, bool>> predicate, object obj)
        {
            Utils.CheckNull(predicate, "predicate");
            Utils.CheckNull(obj, "obj");

            var type = typeof(T);
            var values = DelegateCreateManage.GetGetValuesDelegate(obj.GetType())(obj);
            EntityDescriptor entityDescriptor = EntityDescriptor.GetEntityDescriptor(type);

            IList<KeyValuePair<string, object>> entityVals = new List<KeyValuePair<string, object>>(values.Count);

            foreach (var kv in values)
            {
                bool exists = false;
                foreach (var item in entityDescriptor.MapMembers)
                {
                    if (item.Key.Name == kv.Key)
                    {
                        if (entityVals.Any(a => a.Key == item.Value.ColumnName))
                            throw new Exception("存在多个 Map 列名相同的成员：" + item.Value.ColumnName);
                        entityVals.Add(new KeyValuePair<string, object>(item.Value.ColumnName, kv.Value));
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                    throw new Exception(string.Format("实体 {0} 不存在名为 {1} 的成员 ", entityDescriptor.EntityType.Name, kv.Key));
            }

            QueryHelper queryHelper = new Query.QueryHelper(this._dbProvider);
            IDictionary<string, object> parameters = queryHelper.Parameters;

            MyExpressionVisitor3 visitor = new MyExpressionVisitor3(queryHelper, entityDescriptor);
            var dbExp = visitor.Visit(predicate);

            string sql = this._dbProvider.GetSqlBuilder().BuildSql_Update(entityDescriptor.TableName, entityVals, _dbProvider.TranslateDbExpression(dbExp), ref parameters);

            return this.DatabaseContext.ExecuteNonQuery(sql, parameters);
        }

        public virtual int Delete<T>(T entity)
        {
            Utils.CheckNull(entity, "entity");

            var type = entity.GetType();
            EntityDescriptor entityDescriptor = EntityDescriptor.GetEntityDescriptor(type);
            string wherePart = string.Empty;

            List<KeyValuePair<string, object>> pkValues = this.GetEntityPKValues(entity, entityDescriptor);

            IDictionary<string, object> parameters = new Dictionary<string, object>(pkValues.Count);
            string sql = this._dbProvider.GetSqlBuilder().BuildSql_Delete(entityDescriptor.TableName, pkValues, ref parameters);
            return this.DatabaseContext.ExecuteNonQuery(sql, parameters);
        }
        public virtual int Delete<T>(Expression<Func<T, bool>> predicate)
        {
            Utils.CheckNull(predicate, "predicate");

            var type = typeof(T);
            EntityDescriptor entityDescriptor = EntityDescriptor.GetEntityDescriptor(type);

            QueryHelper queryHelper = new Query.QueryHelper(this._dbProvider);

            MyExpressionVisitor3 visitor = new MyExpressionVisitor3(queryHelper, entityDescriptor);
            var dbExp = visitor.Visit(predicate);

            string sql = this._dbProvider.GetSqlBuilder().BuildSql_Delete(entityDescriptor.TableName, _dbProvider.TranslateDbExpression(dbExp));
            return this.DatabaseContext.ExecuteNonQuery(sql, queryHelper.Parameters);
        }

        public virtual int ExecuteNonQuery(string sql, IDictionary<string, object> parameters)
        {
            Utils.CheckNull(sql, "sql");
            return this.DatabaseContext.ExecuteNonQuery(sql, parameters);
        }
        public virtual object ExecuteScalar(string sql, IDictionary<string, object> parameters)
        {
            Utils.CheckNull(sql, "sql");
            return this.DatabaseContext.ExecuteScalar(sql, parameters);
        }
        public virtual IDataReader ExecuteReader(string sql, IDictionary<string, object> parameters)
        {
            Utils.CheckNull(sql, "sql");
            return this.DatabaseContext.ExecuteInternalReader(CommandType.Text, sql, parameters);
        }
        public virtual DataTable ExecuteDataTable(string sql, IDictionary<string, object> parameters)
        {
            Utils.CheckNull(sql, "sql");
            return this.DatabaseContext.ExecuteDataTable(sql, parameters);
        }

        public abstract IDbConnection CreateConnection();

        public virtual void BeginTran()
        {
            this.DatabaseContext.BeginTransaction();
        }
        public virtual void BeginTran(IsolationLevel il)
        {
            this.DatabaseContext.BeginTransaction(il);
        }
        public virtual void CommitTran()
        {
            this.DatabaseContext.CommitTransaction();
        }
        public virtual void RollbackTran()
        {
            this.DatabaseContext.RollbackTransaction();
        }

        public virtual void Dispose()
        {
            if (this._databaseContext != null)
                this._databaseContext.Dispose();
        }


        private List<KeyValuePair<string, object>> GetEntityPKValues(object entity, EntityDescriptor entityDescriptor)
        {
            if (entityDescriptor.PrimaryKeyMembers.Count == 0)
            {
                throw new Exception(string.Format("实体 {0} 未设置主键", entityDescriptor.EntityType.Name));
            }

            List<KeyValuePair<string, object>> pkValues = new List<KeyValuePair<string, object>>(entityDescriptor.PrimaryKeyMembers.Count);
            foreach (var pkMember in entityDescriptor.PrimaryKeyMembers)
            {
                var pkValue = pkMember.MemberInfo.GetPropertyOrFieldValue(entity);

                if (pkValue == null)
                    throw new Exception(string.Format("主键 {0} 值为 null", pkMember.MemberInfo.Name));

                pkValues.Add(new KeyValuePair<string, object>(pkMember.ColumnName, pkValue));
            }

            return pkValues;
        }

    }
}
