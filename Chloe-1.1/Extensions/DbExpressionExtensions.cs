using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Extensions
{
    internal static class DbExpressionExtensions
    {
        public static bool IsReturnCSharpBoolean(this DbMemberExpression exp)
        {
            DbExpression prevExp = exp.Expression;
            DbMemberExpression memberExp = prevExp as DbMemberExpression;
            while (memberExp != null)
            {
                prevExp = memberExp.Expression;
                memberExp = prevExp as DbMemberExpression;
            }

            DbExpressionType nodeType = prevExp.NodeType;

            if (nodeType == DbExpressionType.Parameter || nodeType == DbExpressionType.Constant || nodeType == DbExpressionType.ColumnAccess)
            {
                return true;
            }
            else
                return false;
        }

        public static bool TryEvaluate(this DbMemberExpression exp, out DbExpression val)
        {
            val = null;
            if (!exp.CanEvaluate())
                return false;

            //求值
            val = exp.Evaluate();
            return true;
        }
        public static bool TryEvaluate(this DbMemberExpression exp, out object val)
        {
            val = null;
            if (!exp.CanEvaluate())
                return false;

            //求值
            val = exp.Evaluate();
            return true;
        }

        public static bool CanEvaluate(this DbMemberExpression memberExpression)
        {
            if (memberExpression == null)
                throw new ArgumentNullException("memberExpression");

            do
            {
                DbExpression prevExp = memberExpression.Expression;

                // prevExp == null 表示是静态成员
                if (prevExp == null || prevExp is DbConstantExpression)
                    return true;

                DbMemberExpression memberExp = prevExp as DbMemberExpression;
                if (memberExp == null)
                    return false;
                else
                    memberExpression = memberExp;

            } while (true);
        }

        /// <summary>
        /// 对 memberExpression 进行求值，如果计算结果为 null 则返回 Value 为 null 的 DbConstantExpression 一个对象，否则返回 DbParameterExpression
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static DbExpression Evaluate(this DbMemberExpression memberExpression)
        {
            DbExpression ret = null;
            //求值
            object val = memberExpression.GetMemberValue();

            if (val == null)
                ret = new DbConstantExpression(null, memberExpression.Type);
            else
                ret = new DbParameterExpression(val, memberExpression.Type);
            return ret;
        }

        public static bool IsNullDbConstantExpression(this DbExpression exp)
        {
            return exp.NodeType == DbExpressionType.Constant && ((DbConstantExpression)exp).Value == null;
        }

        public static object GetMemberValue(this DbMemberExpression exp)
        {
            object memberVal = null;
            var stack = exp.Reverse();
            exp = stack.Peek();

            var c = exp.Expression as DbConstantExpression;
            if (c != null)
            {
                exp.TryGetFieldOrPropertyValue(c.Value, out memberVal);
                goto getValue;
            }
            else if (exp.Expression == null)//说明是静态成员
            {
                goto getValue;
            }
            else
                throw new NotSupportedException(exp.Expression.ToString());

        getValue:
            stack.Pop();
            if (stack.Count > 0)
            {
                foreach (var rec in stack)
                {
                    rec.TryGetFieldOrPropertyValue(memberVal, out memberVal);
                }
            }

            return memberVal;
        }
        public static Stack<DbMemberExpression> Reverse(this DbMemberExpression exp)
        {
            var stack = new Stack<DbMemberExpression>();
            stack.Push(exp);
            while ((exp = exp.Expression as DbMemberExpression) != null)
            {
                stack.Push(exp);
            }
            return stack;
        }
        public static bool TryGetFieldOrPropertyValue(this DbMemberExpression exp, object instance, out object memberValue)
        {
            var result = false;
            memberValue = null;

            try
            {
                if (exp.Member.MemberType
                    == MemberTypes.Field)
                {
                    memberValue = ((FieldInfo)exp.Member).GetValue(instance);
                    result = true;
                }
                else if (exp.Member.MemberType
                         == MemberTypes.Property)
                {
                    memberValue = ((PropertyInfo)exp.Member).GetValue(instance, null);
                    result = true;
                }
                return result;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public static DbExpression StripConvert(DbExpression exp)
        {
            DbExpression operand = exp;
            while (operand.NodeType == DbExpressionType.Convert)
            {
                operand = ((DbConvertExpression)operand).Operand;
            }
            return operand;
        }

    }
}
