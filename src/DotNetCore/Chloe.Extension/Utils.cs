using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Extension
{
    static class Utils
    {
        public static void CheckNull(object obj, string paramName = null)
        {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }
        public static bool AreEqual(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 != null)
            {
                return obj1.Equals(obj2);
            }

            if (obj2 != null)
            {
                return obj2.Equals(obj1);
            }

            return object.Equals(obj1, obj2);
        }
        public static Expression MakeWrapperAccess(object value, Type targetType)
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
        public static Task<T> MakeTask<T>(Func<T> func)
        {
            var task = new Task<T>(func);
            task.Start();
            return task;
        }
    }
}
