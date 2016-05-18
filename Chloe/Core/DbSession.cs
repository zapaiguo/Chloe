using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Core
{
    class DbSession : IDbSession
    {
        DbContext _dbContext;
        internal DbSession(DbContext dbContext)
        {
            this._dbContext = dbContext;
        }
        public bool IsInTransaction { get { return this._dbContext.DbSession.IsInTransaction; } }
        public int ExecuteNonQuery(string sql, IDictionary<string, object> parameters = null)
        {
            Utils.CheckNull(sql, "sql");
            return this._dbContext.DbSession.ExecuteNonQuery(sql, parameters);
        }
        public object ExecuteScalar(string sql, IDictionary<string, object> parameters = null)
        {
            Utils.CheckNull(sql, "sql");
            return this._dbContext.DbSession.ExecuteScalar(sql, parameters);
        }
        public IDataReader ExecuteReader(string sql, IDictionary<string, object> parameters = null)
        {
            Utils.CheckNull(sql, "sql");
            return this._dbContext.DbSession.ExecuteInternalReader(sql, parameters, CommandType.Text);
        }

        public void BeginTransaction()
        {
            this._dbContext.DbSession.BeginTransaction();
        }
        public void BeginTransaction(IsolationLevel il)
        {
            this._dbContext.DbSession.BeginTransaction(il);
        }
        public void CommitTransaction()
        {
            this._dbContext.DbSession.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            this._dbContext.DbSession.RollbackTransaction();
        }

        public void Dispose()
        {
            this._dbContext.Dispose();
        }
    }
}
