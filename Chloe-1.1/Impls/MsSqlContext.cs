using Chloe.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Impls
{
    public class MsSqlContext : DbContext
    {
        string _connString;
        public MsSqlContext(string connString)
            : base(CreateDbServiceProvider(connString))
        {
            this._connString = connString;
        }

        static IDbServiceProvider CreateDbServiceProvider(string connString)
        {
            DbServiceProvider provider = new DbServiceProvider(connString);
            return provider;
        }
    }
}
