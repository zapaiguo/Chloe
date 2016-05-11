using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Query.Mapping
{
    public class MappingData
    {
        public MappingData()
        {
        }
        public IObjectActivatorCreator MappingEntity { get; set; }
        public DbSqlQueryExpression SqlQuery { get; set; }
    }
}
