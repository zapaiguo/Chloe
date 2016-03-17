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
        public List<DbColumnExpression> Columns { get; set; }
        public TablePart Table { get; set; }
        public DbExpression Where { get; set; }
        public List<DbExpression> Groups { get; set; }
        public DbExpression Having { get; set; }
        public List<OrderPart> Orders { get; set; }

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
    }
}
