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
        public static DeriveType GetMemberExpDeriveType(this MemberExpression exp)
        {
            MemberExpression prevExp = exp.Expression as MemberExpression;
            if (prevExp != null)
            {
                return prevExp.GetMemberExpDeriveType();
            }
            if (exp.Expression is ParameterExpression)
            {
                return DeriveType.Parameter;
            }
            else if (exp.Expression is ConstantExpression)
            {
                return DeriveType.Constant;
            }
            else
                return DeriveType.Unknown;
        }

        public static bool IsConstantNull(this Expression exp)
        {
            return ((exp is ConstantExpression) && ((ConstantExpression)exp).Value == null);
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
            var stack = new Stack<MemberExpression>();
            stack.Push(exp);
            while ((exp = exp.Expression as MemberExpression) != null)
            {
                stack.Push(exp);
            }
            exp = stack.Peek();

            var c = exp.Expression as ConstantExpression;

            if (c != null)
            {
                exp.TryGetFieldOrPropertyValue(c.Value, out memberVal);

                stack.Pop();
                if (stack.Count > 0)
                {
                    foreach (var rec in stack)
                    {
                        rec.TryGetFieldOrPropertyValue(memberVal, out memberVal);
                    }
                }
            }
            else
                throw new NotSupportedException(exp.Expression.ToString());

            return memberVal;
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
