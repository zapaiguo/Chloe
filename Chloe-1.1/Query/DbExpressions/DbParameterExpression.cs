using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public class DbParameterExpression : DbExpression
    {
        object _value;
        Type _type;
        public DbParameterExpression(object value)
            : base(DbExpressionType.Parameter)
        {
            Utils.CheckNull(value, "value");

            this._value = value;
            this._type = value.GetType();
        }
        public DbParameterExpression(object value, Type type)
            : base(DbExpressionType.Parameter)
        {
            Utils.CheckNull(value, "value");
            Utils.CheckNull(type, "type");

            this._value = value;
            this._type = type;
        }

        public override Type Type
        {
            get
            {
                return this._type;
            }
        }
        public object Value { get { return this._value; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

    }
}
