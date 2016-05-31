using Chloe.DbExpressions;
using Chloe.Query;
using Chloe.Query.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Core.Visitors
{
    public abstract class AbstractDbExpressionVisitor : DbExpressionVisitor<ISqlState>
    {
        public abstract List<DbParam> Parameters { get; }
    }
}
