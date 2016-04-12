using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public class DbConstantExpression : DbExpression
    {
        private object _value;
        public DbConstantExpression(object value)
            : base(DbExpressionType.Constant)
        {
            _value = value;
        }

        public object Value { get { return _value; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
