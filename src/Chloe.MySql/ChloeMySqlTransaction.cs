using Chloe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.MySql
{
    public class ChloeMySqlTransaction : DbTransactionDecorator, IDbTransaction
    {
        ChloeMySqlConnection _connection;
        public ChloeMySqlTransaction(ChloeMySqlConnection connection) : base(connection.PersistedConnection.BeginTransaction())
        {
            this._connection = connection;
        }
        public ChloeMySqlTransaction(ChloeMySqlConnection connection, IsolationLevel il) : base(connection.PersistedConnection.BeginTransaction(il))
        {
            this._connection = connection;
        }

        public override IDbConnection Connection { get { return this._connection; } }
    }
}
