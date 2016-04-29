using Chloe.Utility;
using System.Reflection;

namespace Chloe.DbExpressions
{
    public class DbEqualExpression : DbBinaryExpression
    {
        public DbEqualExpression(DbExpression left, DbExpression right)
            : this(left, right, null)
        {
        }
        public DbEqualExpression(DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.Equal, UtilConstants.TypeOfBoolean, left, right, method)
        {
        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
