using Chloe.Core.Visitors;
using Chloe.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Chloe.Infrastructure
{
    public interface IDbContextServiceProvider
    {
        IDbConnection CreateConnection();
        IDbExpressionTranslator CreateDbExpressionTranslator();
    }
}
