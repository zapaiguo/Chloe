using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Extensions
{
    internal static class ExpressionExtensions
    {
        public static bool IsDerivedFromParameter(this MemberExpression exp)
        {
            ParameterExpression p;
            return IsDerivedFromParameter(exp, out p);
        }

        public static bool IsDerivedFromParameter(this MemberExpression exp, out ParameterExpression p)
        {
            Expression prevExp = exp.Expression;
            MemberExpression memberExp = prevExp as MemberExpression;
            while (memberExp != null)
            {
                prevExp = memberExp.Expression;
                memberExp = prevExp as MemberExpression;
            }

            p = prevExp as ParameterExpression;

            if (p != null)
                return true;
            else
                return false;
        }

        public static Expression StripQuotes(this Expression exp)
        {
            while (exp.NodeType == ExpressionType.Quote)
            {
                exp = ((UnaryExpression)exp).Operand;
            }
            return exp;
        }

        public static object GetMemberValue(this MemberExpression exp)
        {
            object memberVal = null;
            var stack = exp.Reverse();
            exp = stack.Peek();

            var c = exp.Expression as ConstantExpression;
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
        public static Stack<MemberExpression> Reverse(this MemberExpression exp)
        {
            var stack = new Stack<MemberExpression>();
            stack.Push(exp);
            while ((exp = exp.Expression as MemberExpression) != null)
            {
                stack.Push(exp);
            }
            return stack;
        }

        public static bool TryGetFieldOrPropertyValue(this MemberExpression exp, object instance, out object memberValue)
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

    }
}
