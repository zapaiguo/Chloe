using Chloe.InternalExtensions;
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

        public static string GetParameterPrefix(IDbContext dbContext)
        {
            Type dbContextType = dbContext.GetType();
            while (true)
            {
                if (dbContextType == null)
                    break;

                string dbContextTypeName = dbContextType.Name;
                switch (dbContextTypeName)
                {
                    case "MsSqlContext":
                    case "SQLiteContext":
                        return "@";
                    case "MySqlContext":
                        return "?";
                    case "OracleContext":
                        return ":";
                    default:
                        dbContextType = dbContextType.BaseType;
                        break;
                }
            }

            throw new NotSupportedException(dbContext.GetType().FullName);
        }
        public static DbParam[] BuildParams(IDbContext dbContext, object parameter)
        {
            List<DbParam> parameters = new List<DbParam>();

            if (parameter != null)
            {
                string parameterPrefix = GetParameterPrefix(dbContext);
                Type parameterType = parameter.GetType();
                var props = parameterType.GetProperties();
                foreach (var prop in props)
                {
                    if (prop.GetGetMethod() == null)
                    {
                        continue;
                    }

                    object value = ReflectionExtension.GetMemberValue(prop, parameter);

                    string paramName = parameterPrefix + prop.Name;

                    DbParam p = new DbParam(paramName, value, prop.PropertyType);
                    parameters.Add(p);
                }
            }

            return parameters.ToArray();
        }

        public static Expression StripConvert(this Expression exp)
        {
            Expression operand = exp;
            while (operand.NodeType == ExpressionType.Convert || operand.NodeType == ExpressionType.ConvertChecked)
            {
                operand = ((UnaryExpression)operand).Operand;
            }
            return operand;
        }
    }
}
