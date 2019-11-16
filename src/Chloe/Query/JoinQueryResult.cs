using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Query
{
    public class JoinQueryResult
    {
        public IObjectModel ResultModel { get; set; }
        public DbJoinTableExpression JoinTable { get; set; }
    }
}
