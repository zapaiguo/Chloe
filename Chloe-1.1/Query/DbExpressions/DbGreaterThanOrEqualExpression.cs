using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query.DbExpressions
{
    public class DbGreaterThanOrEqualExpression : DbBinaryExpression
    {
        internal DbGreaterThanOrEqualExpression(DbExpression left, DbExpression right)
            : this(left, right, null)
        {

        }
        internal DbGreaterThanOrEqualExpression(DbExpression left, DbExpression right, MethodInfo method)
            : base(DbExpressionType.GreaterThanOrEqual, UtilConstants.TypeOfBoolean, left, right, method)
        {

        }

        public override T Accept<T>(DbExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

}
