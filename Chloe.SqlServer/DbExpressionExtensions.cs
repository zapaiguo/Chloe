using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.SqlServer
{
    static class DbExpressionExtensions
    {
        /// <summary>
        /// 尝试将 exp 转换成 DbParameterExpression。
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool TryParseToParameterExpression(this DbMemberExpression exp, out DbParameterExpression val)
        {
            val = null;
            if (!exp.CanEvaluate())
                return false;

            //求值
            val = exp.ParseToParameterExpression();
            return true;
        }
        /// <summary>
        /// 尝试将 exp 转换成 DbParameterExpression。
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static DbExpression ParseDbExpression(this DbExpression exp)
        {
            DbExpression stripedExp = DbExpressionHelper.StripInvalidConvert(exp);

            DbExpression tempExp = stripedExp;

            List<DbConvertExpression> cList = null;
            while (tempExp.NodeType == DbExpressionType.Convert)
            {
                if (cList == null)
                    cList = new List<DbConvertExpression>();

                DbConvertExpression c = (DbConvertExpression)tempExp;
                cList.Add(c);
                tempExp = c.Operand;
            }

            if (tempExp.NodeType == DbExpressionType.Constant || tempExp.NodeType == DbExpressionType.Parameter)
                return stripedExp;

            if (tempExp.NodeType == DbExpressionType.MemberAccess)
            {
                DbParameterExpression val;
                if (DbExpressionExtensions.TryParseToParameterExpression((DbMemberExpression)tempExp, out val))
                {
                    if (cList != null)
                    {
                        if (val.Value == DBNull.Value)//如果是 null，则不需要 Convert 了，在数据库里没意义
                            return val;

                        DbConvertExpression c = null;
                        for (int i = cList.Count - 1; i > -1; i--)
                        {
                            DbConvertExpression item = cList[i];
                            c = new DbConvertExpression(item.Type, val);
                        }

                        return c;
                    }

                    return val;
                }
            }

            return stripedExp;
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
        /// <returns>如果计算结果为 null 则返回 Value 为 null 的 DbConstantExpression 一个对象，否则返回 DbParameterExpression</returns>
        public static DbParameterExpression ParseToParameterExpression(this DbMemberExpression memberExpression)
        {
            DbParameterExpression ret = null;
            //求值
            object val = memberExpression.GetMemberValue();

            if (val == null)
                ret = new DbParameterExpression(DBNull.Value);
            else
                ret = new DbParameterExpression(val);
            return ret;
        }

        /// <summary>
        /// 判定 exp 返回值肯定是 null
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static bool AffirmExpressionRetValueIsNull(this DbExpression exp)
        {
            exp = DbExpressionHelper.StripConvert(exp);

            if (exp.NodeType == DbExpressionType.Constant && ((DbConstantExpression)exp).Value == null)
                return true;

            if (exp.NodeType == DbExpressionType.Parameter && ((DbParameterExpression)exp).Value == DBNull.Value)
                return true;

            return false;
        }
        /// <summary>
        /// 判定 exp 返回值肯定不是 null
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static bool AffirmExpressionRetValueIsNotNull(this DbExpression exp)
        {
            exp = DbExpressionHelper.StripConvert(exp);

            if (exp.NodeType == DbExpressionType.Constant && ((DbConstantExpression)exp).Value != null)
                return true;

            if (exp.NodeType == DbExpressionType.Parameter && ((DbParameterExpression)exp).Value != DBNull.Value)
                return true;

            return false;
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
    }
}
