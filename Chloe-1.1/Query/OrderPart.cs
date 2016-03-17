using Chloe.Query.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public class OrderPart
    {
        public OrderPart(DbExpression dbExpression, OrderType orderType)
        {
            this.DbExpression = dbExpression;
            this.OrderType = orderType;
        }
        public DbExpression DbExpression { get; set; }
        public OrderType OrderType { get; set; }
    }
}
