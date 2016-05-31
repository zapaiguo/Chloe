using Chloe.Core;
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
        int ExecuteNonQuery(string sql);
        int ExecuteNonQuery(string sql, params DbParam[] parameters);
        object ExecuteScalar(string sql);
        object ExecuteScalar(string sql, params DbParam[] parameters);
        IDataReader ExecuteReader(string sql);
        IDataReader ExecuteReader(string sql, params DbParam[] parameters);

        void BeginTransaction();
        void BeginTransaction(IsolationLevel il);
        void CommitTransaction();
        void RollbackTransaction();

    }
}
