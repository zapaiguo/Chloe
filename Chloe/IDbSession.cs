using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe
{
    public interface IDbSession : IDisposable
    {
        bool IsInTransaction { get; }
        int ExecuteNonQuery(string sql, IDictionary<string, object> parameters);
        object ExecuteScalar(string sql, IDictionary<string, object> parameters);
        IDataReader ExecuteReader(string sql, IDictionary<string, object> parameters);

        void BeginTransaction();
        void BeginTransaction(IsolationLevel il);
        void CommitTransaction();
        void RollbackTransaction();

    }
}
