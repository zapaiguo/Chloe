using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public class DbConvertExpression : DbExpression
    {
        MethodInfo _method;
        DbExpression _operand;

        public DbConvertExpression(Type type, DbExpression operand, MethodInfo method)
            : base(DbExpressionType.Convert, type)
        {
            this._operand = operand;
            this._method = method;
        }
        public MethodInfo Method { get { return this._method; } }
        public virtual DbExpression Operand { get { return this._operand; } }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
