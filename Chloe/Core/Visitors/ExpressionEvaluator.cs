using Chloe.Extensions;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Core.Visitors
{
    public class ExpressionEvaluator : ExpressionVisitor<object>
    {
        static ExpressionEvaluator _evaluator = new ExpressionEvaluator();
        public static object Evaluate(Expression exp)
        {
            return _evaluator.Visit(exp);
        }
        protected override object VisitMemberAccess(MemberExpression exp)
        {
            var val = this.Visit(exp.Expression);

            if (val == null)
                ThrowHelper.ThrowNullReferenceException();

            if (exp.Member.MemberType == MemberTypes.Property)
            {
                var pro = (PropertyInfo)exp.Member;
                return pro.GetValue(val);
            }
            else if (exp.Member.MemberType == MemberTypes.Field)
            {
                var field = (FieldInfo)exp.Member;
                return field.GetValue(val);
            }

            throw new NotSupportedException();
        }
        protected override object VisitUnary_Not(UnaryExpression exp)
        {
            var operandValue = this.Visit(exp.Operand);

            if ((bool)operandValue == true)
                return false;

            return true;
        }
        protected override object VisitUnary_Convert(UnaryExpression exp)
        {
            var operandValue = this.Visit(exp.Operand);

            //(int)null
            if (operandValue == null)
            {
                //(int)null
                if (exp.Type.IsValueType && !Utils.IsNullable(exp.Type))
                    ThrowHelper.ThrowNullReferenceException();

                return null;
            }

            Type operandValueType = operandValue.GetType();

            if (exp.Type == operandValueType || exp.Type.IsAssignableFrom(operandValueType))
            {
                return operandValue;
            }

            Type unType;

            if (Utils.IsNullable(exp.Type, out unType))
            {
                //(int?)int
                if (unType == operandValueType)
                {
                    var constructor = exp.Type.GetConstructor(new Type[] { operandValueType });
                    var val = constructor.Invoke(new object[] { operandValue });
                    return val;
                }
                else
                {
                    //如果不等，则诸如：(long?)int / (long?)int? 
                    //则转成：(long?)((long)int) / (long?)((long)int?)
                    var c = Expression.MakeUnary(ExpressionType.Convert, Expression.Constant(operandValue), unType);
                    var cc = Expression.MakeUnary(ExpressionType.Convert, c, exp.Type);
                    return this.Visit(cc);
                }
            }

            //(int)int?
            if (Utils.IsNullable(operandValueType, out unType))
            {
                if (unType == exp.Type)
                {
                    var pro = operandValueType.GetProperty("Value");
                    var val = pro.GetValue(operandValue);
                    return val;
                }
                else
                {
                    //如果不等，则诸如：(long)int? 
                    //则转成：(long)((long)int) 处理
                    var c = Expression.MakeUnary(ExpressionType.Convert, Expression.Constant(operandValue), unType);
                    var cc = Expression.MakeUnary(ExpressionType.Convert, c, exp.Type);
                    return this.Visit(cc);
                }
            }

            //(long)int
            if (operandValue is IConvertible)
            {
                return Convert.ChangeType(operandValue, exp.Type);
            }

            ThrowHelper.ThrowNotSupportedException(string.Format("不支持将类型 {0} 转换成 {1}", operandValueType.FullName, exp.Type.FullName));

            return null;
        }
        protected override object VisitConstant(ConstantExpression exp)
        {
            return exp.Value;
        }
    }
}
