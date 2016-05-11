using Chloe.Utility;
using System;

namespace Chloe.DbExpressions
{
    public class DbConstantExpression : DbExpression
    {
        object _value;
        Type _type;

        public DbConstantExpression(object value)
            : base(DbExpressionType.Constant)
        {
            this._value = value;

            if (value != null)
                this._type = value.GetType();
            else
                this._type = UtilConstants.TypeOfObject;
        }

        public DbConstantExpression(object value, Type type)
            : base(DbExpressionType.Constant)
        {
            Utils.CheckNull(type);

            if (value != null)
            {
                Type t = value.GetType();

                if (!type.IsAssignableFrom(t))
                    throw new ArgumentException();
            }

            this._value = value;
            this._type = type;
        }

        public override Type Type { get { return this._type; } }
        public object Value { get { return this._value; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
