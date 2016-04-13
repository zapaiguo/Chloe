using System;

namespace Chloe.DbExpressions
{
    public class DbConstantExpression : DbExpression
    {
        object _value;
        public DbConstantExpression(object value, Type type)
            : base(DbExpressionType.Constant, type)
        {
            this._value = value;
        }

        public object Value { get { return this._value; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
