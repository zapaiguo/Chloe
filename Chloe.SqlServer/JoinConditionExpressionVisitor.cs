using Chloe.Extensions;
using Chloe.DbExpressions;
using System;
using Chloe.Query;

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
            SqlState state = null;
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = DbExpressionExtensions.ParseDbExpression(left);
            right = DbExpressionExtensions.ParseDbExpression(right);

            //明确 left right 其中一边一定为 null
            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(right))
            {
                state = new SqlState(2);
                state.Append(left.Accept(this), " IS NULL");
                return state;
            }

            if (DbExpressionExtensions.AffirmExpressionRetValueIsNull(left))
            {
                state = new SqlState(2);
                state.Append(right.Accept(this), " IS NULL");
                return state;
            }

            ISqlState leftState = left.Accept(this);
            ISqlState rightState = right.Accept(this);

            state = new SqlState(3);
            state.Append(leftState, " = ", rightState);

            return state;
        }

        public override ISqlState Visit(DbParameterExpression exp)
        {
            return exp.Accept(this._visitor);
        }

    }
}
