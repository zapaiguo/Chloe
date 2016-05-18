using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe
{
    public interface IDbSession : IDisposable
    {
        bool IsInTransaction { get; }
        int ExecuteNonQuery(string sql, IDictionary<string, object> parameters = null);
        object ExecuteScalar(string sql, IDictionary<string, object> parameters = null);
        IDataReader ExecuteReader(string sql, IDictionary<string, object> parameters = null);

        void BeginTransaction();
        void BeginTransaction(IsolationLevel il);
        void CommitTransaction();
        void RollbackTransaction();

    }
}
