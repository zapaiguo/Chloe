using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public class DbDivideExpression : DbBinaryExpression
    {
        internal DbDivideExpression(Type type, DbExpression left, DbExpression right)
            : this(type, left, right, null)
        {

        }
        internal DbDivideExpression(Type type, DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.Divide, type, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}
