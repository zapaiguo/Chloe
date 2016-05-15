using Chloe.Extensions;
using Chloe.DbExpressions;
using System;
using Chloe.Query;
using Chloe.Core;

namespace Chloe.SqlServer
{
    class JoinConditionExpressionVisitor : SqlExpressionVisitor
    {
        SqlExpressionVisitor _visitor = null;

        public JoinConditionExpressionVisitor(SqlExpressionVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            this._visitor = visitor;
        }

        public override ISqlState Visit(DbEqualExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = DbExpressionExtensions.ParseDbExpression(left);
            right = DbExpressionExtensions.ParseDbExpression(right);

            //明确 left right 其中一边一定为 null
            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(right))
            {
                return SqlState.Create(left.Accept(this), " IS NULL");
            }

            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(left))
            {
                return SqlState.Create(right.Accept(this), " IS NULL");
            }

            ISqlState leftState = left.Accept(this);
            ISqlState rightState = right.Accept(this);

            return SqlState.Create(leftState, " = ", rightState);
        }
    }
}
