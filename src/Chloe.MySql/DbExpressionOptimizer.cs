using Chloe.Annotations;
using Chloe.DbExpressions;
using Chloe.InternalExtensions;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static Chloe.DbExpressions.DbCaseWhenExpression;

namespace Chloe.MySql
{
    public class DbExpressionOptimizer : DbExpressionVisitor
    {
        static DbExpressionOptimizer _optimizer = new DbExpressionOptimizer();

        static KeyDictionary<MemberInfo> _toTranslateMembers = new KeyDictionary<MemberInfo>();
        static DbExpressionOptimizer()
        {
            _toTranslateMembers.Add(UtilConstants.PropertyInfo_String_Length);

            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_Now);
            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_UtcNow);
            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_Today);
            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_Date);

            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_Year);
            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_Month);
            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_Day);
            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_Hour);
            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_Minute);
            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_Second);
            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_Millisecond);
            _toTranslateMembers.Add(UtilConstants.PropertyInfo_DateTime_DayOfWeek);

            _toTranslateMembers = _toTranslateMembers.Clone();
        }

        static bool IsConstantOrParameter(DbExpression exp)
        {
            return exp != null && (exp.NodeType == DbExpressionType.Constant || exp.NodeType == DbExpressionType.Parameter);
        }

        public static DbExpression Optimize(DbExpression exp)
        {
            return exp.Accept(_optimizer);
        }

        public override DbExpression Visit(DbMemberExpression exp)
        {
            if (exp.Expression != null)
            {
                DbExpression caller = exp.Expression.Accept(this);
                if (caller != exp.Expression)
                    exp = DbExpression.MemberAccess(exp.Member, caller);
            }

            if (exp.Expression != null)
            {
                if (!IsConstantOrParameter(exp.Expression))
                    return exp;
            }

            MemberInfo member = exp.Member;

            if (_toTranslateMembers.Exists(exp.Member))
                return exp;

            return DbExpression.Parameter(exp.Evaluate(), exp.Type);
        }

        public override DbExpression Visit(DbConvertExpression exp)
        {
            exp = DbExpression.Convert(exp.Operand.Accept(this), exp.Type);

            if (!IsConstantOrParameter(exp.Operand))
            {
                return exp;
            }

            return DbExpression.Parameter(exp.Evaluate(), exp.Type);
        }
        public override DbExpression Visit(DbCoalesceExpression exp)
        {
            exp = new DbCoalesceExpression(exp.CheckExpression.Accept(this), exp.ReplacementValue.Accept(this));

            if (IsConstantOrParameter(exp.CheckExpression) && IsConstantOrParameter(exp.ReplacementValue))
            {
                return DbExpression.Parameter(exp.Evaluate(), exp.Type);
            }

            return exp;
        }

        public override DbExpression Visit(DbMethodCallExpression exp)
        {
            var args = exp.Arguments.Select(a => a.Accept(this)).ToList();
            DbExpression caller = exp.Object;
            if (exp.Object != null)
            {
                caller = exp.Object.Accept(this);
            }

            exp = DbExpression.MethodCall(caller, exp.Method, args);

            if (exp.Object != null)
            {
                if (!IsConstantOrParameter(exp.Object))
                    return exp;
            }

            foreach (var arg in exp.Arguments)
            {
                if (!IsConstantOrParameter(arg))
                    return exp;
            }

            IMethodHandler methodHandler;
            if (SqlGenerator.MethodHandlers.TryGetValue(exp.Method.Name, out methodHandler))
            {
                if (methodHandler.CanProcess(exp))
                {
                    return exp;
                }
            }

            if (exp.Method.IsDefined(typeof(DbFunctionAttribute)))
            {
                return exp;
            }

            return DbExpression.Parameter(exp.Evaluate(), exp.Type);
        }

        public override DbExpression Visit(DbParameterExpression exp)
        {
            return exp;
        }
    }
}
