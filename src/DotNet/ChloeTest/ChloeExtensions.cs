using Chloe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChloeTest
{
    public static class ChloeExtensions
    {
        public static void InsertRange<TEntity>(this IDbContext dbContext, List<TEntity> entities)
        {



        }
    }

    class ParameterTwoExpressionReplacer : ExpressionVisitor
    {
        LambdaExpression _lambda;
        object _replaceObj;
        Expression _expToReplace = null;

        ParameterTwoExpressionReplacer(LambdaExpression lambda, object replaceObj)
        {
            this._lambda = lambda;
            this._replaceObj = replaceObj;
        }

        public static LambdaExpression Replace(LambdaExpression lambda, object replaceObj)
        {
            LambdaExpression ret = new ParameterTwoExpressionReplacer(lambda, replaceObj).Replace();
            return ret;
        }

        LambdaExpression Replace()
        {
            Expression lambdaBody = this._lambda.Body;
            Expression newBody = this.Visit(lambdaBody);

            ParameterExpression firstParameterExp = this._lambda.Parameters[0];
            Type delegateType = typeof(Func<,>).MakeGenericType(firstParameterExp.Type, typeof(bool));
            LambdaExpression newLambda = Expression.Lambda(delegateType, newBody, firstParameterExp);
            return newLambda;
        }

        protected override Expression VisitParameter(ParameterExpression parameter)
        {
            Expression ret = parameter;
            if (parameter == this._lambda.Parameters[1])
            {
                if (this._expToReplace == null)
                    this._expToReplace = MakeWrapperAccess(this._replaceObj, parameter.Type);
                ret = this._expToReplace;
            }

            return ret;
        }

        static Expression MakeWrapperAccess(object value, Type targetType)
        {
            object wrapper;
            Type wrapperType;

            if (value == null)
            {
                if (targetType != null)
                    return Expression.Constant(value, targetType);
                else
                    return Expression.Constant(value, typeof(object));
            }
            else
            {
                Type valueType = value.GetType();
                wrapperType = typeof(ConstantWrapper<>).MakeGenericType(valueType);
                ConstructorInfo constructor = wrapperType.GetConstructor(new Type[] { valueType });
                wrapper = constructor.Invoke(new object[] { value });
            }

            ConstantExpression wrapperConstantExp = Expression.Constant(wrapper);
            Expression ret = Expression.MakeMemberAccess(wrapperConstantExp, wrapperType.GetProperty("Value"));

            if (ret.Type != targetType)
            {
                ret = Expression.Convert(ret, targetType);
            }

            return ret;
        }
    }

    internal class ConstantWrapper<T>
    {
        static readonly PropertyInfo PropertyOfValue = typeof(ConstantWrapper<T>).GetProperty("Value");
        public ConstantWrapper(T value)
        {
            this.Value = value;
        }
        public T Value { get; private set; }
    }

}
