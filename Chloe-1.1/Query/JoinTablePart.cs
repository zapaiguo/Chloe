using Chloe.Query.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public class JoinTablePart : TablePart
    {
        public JoinTablePart(JoinType joinType, DbTableExpression table, DbTableExpression relatedTable, DbExpression condition)
            : base(table)
        {
            this.JoinType = joinType;
            this.RelatedTable = relatedTable;
            this.Condition = condition;
        }
        public DbTableExpression RelatedTable { get; set; }
        public JoinType JoinType { get; set; }
        public DbExpression Condition { get; set; }
    }
}
