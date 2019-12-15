using Chloe.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Chloe.SQLite
{
    class ConcurrentDbConnectionFactory : IDbConnectionFactory
    {
        IDbConnectionFactory _dbConnectionFactory;
        public ConcurrentDbConnectionFactory(IDbConnectionFactory dbConnectionFactory)
        {
            this._dbConnectionFactory = dbConnectionFactory;
        }
        public IDbConnection CreateConnection()
        {
            IDbConnection conn = new ChloeSQLiteConcurrentConnection(this._dbConnectionFactory.CreateConnection());
            return conn;
        }
    }
}
