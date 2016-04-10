using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public class DbOrderSegmentExpression : DbExpression
    {
        public DbOrderSegmentExpression(DbExpression dbExpression, OrderType orderType)
            : base(DbExpressionType.OrderSegment)
        {
            this.DbExpression = dbExpression;
            this.OrderType = orderType;
        }
        public DbExpression DbExpression { get; set; }
        public OrderType OrderType { get; set; }
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
