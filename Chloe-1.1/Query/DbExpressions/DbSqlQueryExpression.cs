using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public class DbSqlQueryExpression : DbExpression
    {
        public DbSqlQueryExpression()
            : base(DbExpressionType.SqlQuery, UtilConstants.TypeOfVoid)
        {
            this.Columns = new List<DbColumnExpression>();
            this.Groups = new List<DbExpression>();
            this.Orders = new List<OrderPart>();
        }
        public int? TakeCount { get; set; }
        public int? SkipCount { get; set; }
        public List<DbColumnExpression> Columns { get; private set; }
        public DbFromTableExpression Table { get; set; }
        public DbExpression Where { get; set; }
        public List<DbExpression> Groups { get; private set; }
        public DbExpression Having { get; set; }
        public List<OrderPart> Orders { get; private set; }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public void UpdateWhereExpression(DbExpression whereExpression)
        {
            if (this.Where == null)
                this.Where = whereExpression;
            else if (whereExpression != null)
                this.Where = new DbAndExpression(this.Where, whereExpression);
        }

        public string GenerateUniqueColumnAlias(string prefix = "C")
        {
            string alias = prefix;
            int i = 0;
            while (this.Columns.Any(a => a.Alias == alias))
            {
                alias = prefix + i.ToString();
                i++;
            }

            return alias;
        }
    }
}
