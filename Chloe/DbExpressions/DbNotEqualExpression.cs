using Chloe.Utility;
using System.Reflection;

namespace Chloe.DbExpressions
{
    public class DbNotEqualExpression : DbBinaryExpression
    {
        internal DbNotEqualExpression(DbExpression left, DbExpression right)
            : this(left, right, null)
        {

        }
        internal DbNotEqualExpression(DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.NotEqual, UtilConstants.TypeOfBoolean, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}
