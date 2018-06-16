using Chloe.DbExpressions;
using Chloe.InternalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Core.Visitors
{
    public class DbExpressionEvaluableJudge : DbExpressionVisitor<bool>
    {
        static DbExpressionEvaluableJudge _judge = new DbExpressionEvaluableJudge();

        DbExpressionEvaluableJudge()
        {
        }

        public static bool CanEvaluate(DbExpression exp)
        {
            if (exp == null)
                throw new ArgumentNullException();

            switch (exp.NodeType)
            {
                case DbExpressionType.Constant:
                case DbExpressionType.MemberAccess:
                case DbExpressionType.Call:
                case DbExpressionType.Not:
                case DbExpressionType.Convert:
                case DbExpressionType.Parameter:
                    return exp.Accept(_judge);
                default:
                    break;
            }

            return false;
        }

        public override bool Visit(DbConstantExpression exp)
        {
            return true;
        }
        public override bool Visit(DbMemberExpression exp)
        {
            if (exp.Expression != null)
            {
                return CanEvaluate(exp.Expression);
            }

            return true;
        }
        public override bool Visit(DbMethodCallExpression exp)
        {
            if (exp.Object != null)
            {
                if (!CanEvaluate(exp.Object))
                    return false;
            }

            foreach (var argument in exp.Arguments)
            {
                if (!CanEvaluate(argument))
                {
                    return false;
                }
            }

            return true;
        }
        public override bool Visit(DbNotExpression exp)
        {
            return CanEvaluate(exp.Operand);
        }
        public override bool Visit(DbConvertExpression exp)
        {
            return CanEvaluate(exp.Operand);
        }
        public override bool Visit(DbParameterExpression exp)
        {
            return true;
        }
    }
}
