using Chloe.Extensions;
using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            return ExpressionExtensions.GetMemberValue(exp);
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

            Type operandType = exp.Operand.Type;

            if (exp.Type == operandType || exp.Type.IsAssignableFrom(operandType) || operandType.IsAssignableFrom(exp.Type))
                return operandValue;

            //(int?)int
            Type unType;
            if (Utils.IsNullable(exp.Type, out unType))
            {
                if (unType == operandType)
                {
                    //Nullable<int>
                    var constructor = exp.Type.GetConstructor(new Type[] { operandType });
                    var val = constructor.Invoke(new object[] { operandValue });
                    return val;
                }
            }

            //(int)int?
            if (Utils.IsNullable(operandType, out unType))
            {
                if (unType == exp.Type)
                {
                    if (operandValue == null)
                        throw new InvalidCastException("可为空的对象必须具有一个值。");

                    var pro = operandType.GetProperty("Value");
                    var val = pro.GetValue(operandValue);
                    return val;
                }
            }

            //(long)int
            if (operandValue is IConvertible)
            {
                return Convert.ChangeType(operandValue, exp.Type);
            }

            throw new NotSupportedException(string.Format("将类型 {0} 转换成 {1}", operandType.FullName, exp.Type.FullName));
        }
        protected override object VisitConstant(ConstantExpression exp)
        {
            return exp.Value;
        }

    }
}
