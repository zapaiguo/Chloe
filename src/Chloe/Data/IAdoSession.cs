using Chloe.Infrastructure.Interception;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Chloe.Data
{
    public delegate void AdoEventHandler<TResult>(IDbCommand command, DbCommandInterceptionContext<TResult> interceptionContext);

    public interface IAdoSession : IDisposable
    {
        IDbConnection DbConnection { get; }
        /// <summary>
        /// 如果未开启事务，则返回 null
        /// </summary>
        IDbTransaction DbTransaction { get; }
        bool IsInTransaction { get; }
        int CommandTimeout { get; set; }

        event AdoEventHandler<IDataReader> OnReaderExecuting;
        event AdoEventHandler<IDataReader> OnReaderExecuted;
        event AdoEventHandler<int> OnNonQueryExecuting;
        event AdoEventHandler<int> OnNonQueryExecuted;
        event AdoEventHandler<object> OnScalarExecuting;
        event AdoEventHandler<object> OnScalarExecuted;

        void Activate();
        /* 表示一次查询完成。在事务中的话不关闭连接，交给 CommitTransaction() 或者 RollbackTransaction() 控制，否则调用 IDbConnection.Close() 关闭连接 */
        void Complete();

        void BeginTransaction(IsolationLevel? il);
        void CommitTransaction();
        void RollbackTransaction();

        IDataReader ExecuteReader(string cmdText, DbParam[] parameters, CommandType cmdType);
        IDataReader ExecuteReader(string cmdText, DbParam[] parameters, CommandType cmdType, CommandBehavior behavior);
        int ExecuteNonQuery(string cmdText, DbParam[] parameters, CommandType cmdType);
        object ExecuteScalar(string cmdText, DbParam[] parameters, CommandType cmdType);
    }
}
