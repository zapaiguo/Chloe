
namespace Chloe.DbExpressions
{
    public class DbOrderSegmentExpression : DbExpression
    {
        OrderType _orderType;
        DbExpression _expression;
        public DbOrderSegmentExpression(DbExpression expression, OrderType orderType)
            : base(DbExpressionType.OrderSegment)
        {
            this._expression = expression;
            this._orderType = orderType;
        }
        public DbExpression DbExpression { get { return this._expression; } }
        public OrderType OrderType { get { return this._orderType; } }
        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
