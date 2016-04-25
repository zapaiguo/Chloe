using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Chloe.Database;
using Chloe.Query;
using Chloe.Core;
using Chloe.Utility;
using Chloe.Infrastructure;

namespace Chloe
{
    public abstract class DbContext : IDbContext, IDisposable
    {
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

        //public virtual string ConnectionString { get { return this.DatabaseContext.DbConnection.ConnectionString; } }
        //public virtual bool IsInTransaction { get { return this.DatabaseContext.IsInTransaction; } }

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
            throw new NotImplementedException();
        }
        public virtual int Update<T>(Expression<Func<T, T>> body, Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public virtual int Delete<T>(T entity)
        {
            throw new NotImplementedException();
        }
        public virtual int Delete<T>(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public virtual int ExecuteNonQuery(string sql, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }
        public virtual object ExecuteScalar(string sql, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }
        public virtual IDataReader ExecuteReader(string sql, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }
        public virtual DataTable ExecuteDataTable(string sql, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        //public virtual void BeginTran()
        //{
        //    this.DatabaseContext.BeginTransaction();
        //}
        //public virtual void BeginTran(IsolationLevel il)
        //{
        //    this.DatabaseContext.BeginTransaction(il);
        //}
        //public virtual void CommitTran()
        //{
        //    this.DatabaseContext.CommitTransaction();
        //}
        //public virtual void RollbackTran()
        //{
        //    this.DatabaseContext.RollbackTransaction();
        //}

        //public virtual void Dispose()
        //{
        //    if (this._databaseContext != null)
        //        this._databaseContext.Dispose();
        //}

        public void Dispose()
        {

        }
    }
}
