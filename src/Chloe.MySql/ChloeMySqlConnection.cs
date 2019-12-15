using Chloe.Data;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.MySql
{
    public class ChloeMySqlConnection : DbConnectionDecorator, IDbConnection, IDisposable
    {
        public ChloeMySqlConnection(IDbConnection dbConnection) : base(dbConnection)
        {
        }

        public IDbConnection PersistedDbConnection { get { return this.PersistedConnection; } }

        public override IDbTransaction BeginTransaction()
        {
            return new ChloeMySqlTransaction(this);
        }
        public override IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return new ChloeMySqlTransaction(this, il);
        }

        public override IDbCommand CreateCommand()
        {
            return new ChloeMySqlCommand(this);
        }
    }
}
