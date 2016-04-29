using Chloe.Utility;
using System;

namespace Chloe.DbExpressions
{
    [System.Diagnostics.DebuggerDisplay("Value = {Value}")]
    public class DbParameterExpression : DbExpression
    {
        object _value;

        /// <summary>
        /// value 不可为空。
        /// </summary>
        /// <param name="value"></param>
        public DbParameterExpression(object value)
            : base(DbExpressionType.Parameter)
        {
            if (value == null)
                throw new ArgumentNullException("value 为 null。请用 DBNull 表示");

            this._value = value;
        }

        public override Type Type { get { return this._value.GetType(); } }
        public object Value { get { return this._value; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
