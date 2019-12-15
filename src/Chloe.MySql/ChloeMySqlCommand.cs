using Chloe.Data;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.MySql
{
    public class ChloeMySqlCommand : DbCommandDecorator, IDbCommand, IDisposable
    {
        ChloeMySqlConnection _connection;
        ChloeMySqlTransaction _transaction;

        public ChloeMySqlCommand(ChloeMySqlConnection connection) : base(connection.PersistedConnection.CreateCommand())
        {
            this._connection = connection;
        }

        public IDbCommand PersistedDbCommand { get { return this.PersistedCommand; } }

        public override IDbConnection Connection
        {
            get
            {
                return this._connection;
            }
            set
            {
                ChloeMySqlConnection conn = (ChloeMySqlConnection)value;
                this._connection = conn;
                this.PersistedCommand.Connection = conn.PersistedConnection;
            }
        }

        public override IDbTransaction Transaction
        {
            get
            {
                return this._transaction;
            }
            set
            {
                ChloeMySqlTransaction tran = (ChloeMySqlTransaction)value;
                this._transaction = tran;
                this.PersistedCommand.Transaction = this._transaction.PersistedTransaction;
            }
        }

        public override IDataReader ExecuteReader()
        {
            return new ChloeMySqlDataReader(this.PersistedCommand.ExecuteReader());
        }
        public override IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return new ChloeMySqlDataReader(this.PersistedCommand.ExecuteReader(behavior));
        }
    }
}
