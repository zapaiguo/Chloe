using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Mapping
{
    public class MappingData
    {
        public MappingData()
        {
        }
        public IObjectActivtorCreator MappingEntity { get; set; }
        public DbSqlQueryExpression SqlQuery { get; set; }
    }
}
