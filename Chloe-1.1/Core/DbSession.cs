using Chloe.Database;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Core
{
    public class DbSession
    {
        InternalDbSession _innerDbSession;
        internal DbSession(InternalDbSession innerDbSession)
        {
            this._innerDbSession = innerDbSession;
        }
        public bool IsInTransaction { get { return this._innerDbSession.IsInTransaction; } }
        public int ExecuteNonQuery(string sql, IDictionary<string, object> parameters)
        {
            Utils.CheckNull(sql, "sql");
            return this._innerDbSession.ExecuteNonQuery(sql, parameters);
        }
        public object ExecuteScalar(string sql, IDictionary<string, object> parameters)
        {
            Utils.CheckNull(sql, "sql");
            return this._innerDbSession.ExecuteScalar(sql, parameters);
        }
        public IDataReader ExecuteReader(string sql, IDictionary<string, object> parameters)
        {
            Utils.CheckNull(sql, "sql");
            return this._innerDbSession.ExecuteInternalReader(CommandType.Text, sql, parameters);
        }

        public void BeginTransaction()
        {
            this._innerDbSession.BeginTransaction();
        }
        public void BeginTransaction(IsolationLevel il)
        {
            this._innerDbSession.BeginTransaction(il);
        }
        public void CommitTransaction()
        {
            this._innerDbSession.CommitTransaction();
        }
        public void RollbackTransaction()
        {
            this._innerDbSession.RollbackTransaction();
        }

    }
}
