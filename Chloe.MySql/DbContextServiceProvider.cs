using Chloe.Core.Visitors;
using Chloe.Infrastructure;
using Chloe.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Chloe.MySql
{
    class DbContextServiceProvider : IDbContextServiceProvider
    {
        IDbConnectionFactory _dbConnectionFactory;
        MySqlContext _msSqlContext;

        public DbContextServiceProvider(IDbConnectionFactory dbConnectionFactory, MySqlContext msSqlContext)
        {
            this._dbConnectionFactory = dbConnectionFactory;
            this._msSqlContext = msSqlContext;
        }
        public IDbConnection CreateConnection()
        {
            return this._dbConnectionFactory.CreateConnection();
        }
        public IDbExpressionTranslator CreateDbExpressionTranslator()
        {
            return DbExpressionTranslator.Instance;
        }
    }
}
