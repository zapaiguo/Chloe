using System;
using System.Data;

namespace Chloe
{
    public interface IDbSession : IDisposable
    {
        IDbContext DbContext { get; }
        bool IsInTransaction { get; }

        int ExecuteNonQuery(string cmdText, params DbParam[] parameters);
        int ExecuteNonQuery(string cmdText, CommandType cmdType, int? cmdTimeout, params DbParam[] parameters);

        object ExecuteScalar(string cmdText, params DbParam[] parameters);
        object ExecuteScalar(string cmdText, CommandType cmdType, int? cmdTimeout, params DbParam[] parameters);

        IDataReader ExecuteReader(string cmdText, params DbParam[] parameters);
        IDataReader ExecuteReader(string cmdText, CommandType cmdType, int? cmdTimeout, params DbParam[] parameters);

        void BeginTransaction();
        void BeginTransaction(IsolationLevel il);
        void CommitTransaction();
        void RollbackTransaction();
    }
}
