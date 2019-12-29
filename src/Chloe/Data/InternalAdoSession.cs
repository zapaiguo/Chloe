using Chloe.Core;
using Chloe.Exceptions;
using Chloe.Infrastructure;
using Chloe.Infrastructure.Interception;
using Chloe.InternalExtensions;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Chloe.Data
{
    class InternalAdoSession : AdoSession, IAdoSession, IDisposable
    {
        IDbConnection _dbConnection;

        public InternalAdoSession(IDbConnection conn)
        {
            this._dbConnection = conn;
        }

        public override IDbConnection DbConnection { get { return this._dbConnection; } }
    }
}
