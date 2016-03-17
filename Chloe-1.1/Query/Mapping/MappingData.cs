using Chloe.Query.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.Mapping
{
    public class MappingData
    {
        public MappingData(Type entityType)
        {
            this.EntityType = entityType;
        }
        public Type EntityType { get; set; }
        public MappingMember MappingInfo { get; set; }
        public DbSqlQueryExpression SqlQuery { get; set; }
    }
}
