using Chloe.Query.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public class SqlModel
    {
        public SqlModel()
        {
            this.Columns = new List<DbColumnExpression>();
            this.GroupPart = new List<DbExpression>();
            this.OrderParts = new List<OrderPart>();
        }
        public int? TakeCount { get; set; }
        public int? SkipCount { get; set; }
        public List<DbColumnExpression> Columns { get; set; }
        public TablePart TablePart { get; set; }
        public DbExpression WhereExpression { get; set; }
        public List<DbExpression> GroupPart { get; set; }
        public DbExpression HavingExpression { get; set; }
        public List<OrderPart> OrderParts { get; set; }

        public void UpdateWhereExpression(DbExpression whereExpression)
        {
            if (this.WhereExpression == null)
                this.WhereExpression = whereExpression;
            else if (whereExpression != null)
                this.WhereExpression = new DbAndExpression(this.WhereExpression, whereExpression);
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select ");

            for (int i = 0; i < this.Columns.Count; i++)
            {
                if (i > 0)
                    sb.Append(",");
                var column = this.Columns[i];
                sb.AppendFormat("{0} as {1}", column.Body, column.Alias);
            }

            sb.Append(" from");
            sb.AppendFormat(" {0} as {1}", this.TablePart.Table.Body, this.TablePart.Table.Alias);

            AppendJoinTable(sb, this.TablePart.JoinTables);

            foreach (var item in this.OrderParts)
            {

            }

            return sb.ToString();
        }

        void AppendJoinTable(StringBuilder sb, List<JoinTablePart> joinTableParts)
        {
            foreach (var joinTablePart in joinTableParts)
            {
                string joinString = "";
                if (joinTablePart.JoinType == JoinType.LeftJoin)
                {
                    joinString = " left join";
                }
                else if (joinTablePart.JoinType == JoinType.InnerJoin)
                {
                    joinString = " inner join";

                }
                else if (joinTablePart.JoinType == JoinType.RightJoin)
                {
                    joinString = " right join";
                }

                sb.AppendFormat(" {0} {1} as {2} on {3}", joinString, joinTablePart.Table.Body, joinTablePart.Table.Alias, "1=1");

                this.AppendJoinTable(sb, joinTablePart.JoinTables);
            }
        }
    }
}
