
namespace Chloe.DbExpressions
{
    public class DbOrderSegment
    {
        OrderType _orderType;
        DbExpression _expression;
        public DbOrderSegment(DbExpression expression, OrderType orderType)
        {
            this._expression = expression;
            this._orderType = orderType;
        }
        public DbExpression DbExpression { get { return this._expression; } }
        public OrderType OrderType { get { return this._orderType; } }
    }
}
