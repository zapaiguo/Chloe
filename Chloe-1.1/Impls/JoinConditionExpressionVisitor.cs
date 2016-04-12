using Chloe.Extensions;
using Chloe.DbExpressions;
using System;
using Chloe.Query;

namespace Chloe.Impls
{
    public class JoinConditionExpressionVisitor : SqlExpressionVisitor
    {
        SqlExpressionVisitor _visitor = null;

        public JoinConditionExpressionVisitor(SqlExpressionVisitor visitor)
            : base(1)
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

            DbMemberExpression leftMemberExpression = left as DbMemberExpression;
            //判断是否可求值
            if (leftMemberExpression != null && leftMemberExpression.CanEvaluate())
            {
                left = leftMemberExpression.Evaluate();
            }

            DbMemberExpression rightMemberExpression = right as DbMemberExpression;
            //判断是否可求值
            if (rightMemberExpression != null && rightMemberExpression.CanEvaluate())
            {
                right = rightMemberExpression.Evaluate();
            }

            ISqlState leftState = left.Accept(this);
            ISqlState rightState = right.Accept(this);

            // left right 其中一边为常量 null
            if (right.IsNullDbConstantExpression() || left.IsNullDbConstantExpression())
            {
                string concatString = " IS ";
                state = new SqlState(3);
                state.Append(leftState, concatString, rightState);
                return state;
            }

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
