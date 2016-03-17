using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Query
{
    public abstract class ExpressionVisitor<T>
    {
        protected ExpressionVisitor()
        {
        }

        public virtual T Visit(Expression exp)
        {
            if (exp == null)
                return default(T);
            switch (exp.NodeType)
            {
                case ExpressionType.Not:
                    return this.VisitUnary_Not((UnaryExpression)exp);
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return this.VisitUnary_Convert((UnaryExpression)exp);
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return this.VisitBinary_Add((BinaryExpression)exp);
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return this.VisitBinary_Subtract((BinaryExpression)exp);
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return this.VisitBinary_Multiply((BinaryExpression)exp);
                case ExpressionType.Divide:
                    return this.VisitBinary_Divide((BinaryExpression)exp);
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return this.VisitBinary_And((BinaryExpression)exp);
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return this.VisitBinary_Or((BinaryExpression)exp);
                case ExpressionType.LessThan:
                    return this.VisitBinary_LessThan((BinaryExpression)exp);
                case ExpressionType.LessThanOrEqual:
                    return this.VisitBinary_Divide((BinaryExpression)exp);
                case ExpressionType.GreaterThan:
                    return this.VisitBinary_GreaterThan((BinaryExpression)exp);
                case ExpressionType.GreaterThanOrEqual:
                    return this.VisitBinary_GreaterThanOrEqual((BinaryExpression)exp);
                case ExpressionType.Equal:
                    return this.VisitBinary_Equal((BinaryExpression)exp);
                case ExpressionType.NotEqual:
                    return this.VisitBinary_NotEqual((BinaryExpression)exp);
                case ExpressionType.Coalesce:
                    return this.VisitBinary_Coalesce((BinaryExpression)exp);
                case ExpressionType.Modulo:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                //case ExpressionType.TypeIs:
                //    return this.VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)exp);
                //case ExpressionType.Parameter:
                //    return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);
                    
                //case ExpressionType.New:
                //    return this.VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                    //case ExpressionType.NewArrayBounds:
                    return this.VisitNewArray((NewArrayExpression)exp);
                //case ExpressionType.Invoke:
                //    return this.VisitInvocation((InvocationExpression)exp);
                //case ExpressionType.MemberInit:
                //    return this.VisitMemberInit((MemberInitExpression)exp);
                //case ExpressionType.ListInit:
                //    return this.VisitListInit((ListInitExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        protected abstract T VisitUnary(UnaryExpression u);

        protected abstract T VisitUnary_Not(UnaryExpression u);

        protected abstract T VisitUnary_Convert(UnaryExpression u);

        protected abstract T VisitBinary(BinaryExpression b);

        protected abstract T VisitBinary_Add(BinaryExpression b);

        protected abstract T VisitBinary_Subtract(BinaryExpression b);

        protected abstract T VisitBinary_Multiply(BinaryExpression b);

        protected abstract T VisitBinary_Divide(BinaryExpression b);

        protected abstract T VisitBinary_And(BinaryExpression b);

        protected abstract T VisitBinary_Or(BinaryExpression b);

        protected abstract T VisitConstant(ConstantExpression c);

        protected abstract T VisitBinary_LessThan(BinaryExpression b);

        protected abstract T VisitBinary_LessThanOrEqual(BinaryExpression b);

        protected abstract T VisitBinary_GreaterThan(BinaryExpression b);

        protected abstract T VisitBinary_GreaterThanOrEqual(BinaryExpression b);

        protected abstract T VisitBinary_Equal(BinaryExpression b);

        protected abstract T VisitBinary_NotEqual(BinaryExpression b);

        protected abstract T VisitBinary_Coalesce(BinaryExpression b);

        protected abstract T VisitLambda(LambdaExpression lambda);

        protected abstract T VisitMemberAccess(MemberExpression exp);

        protected abstract T VisitConditional(ConditionalExpression exp);

        protected abstract T VisitMethodCall(MethodCallExpression exp);

        protected abstract T VisitNewArray(NewArrayExpression exp);

    }
}
