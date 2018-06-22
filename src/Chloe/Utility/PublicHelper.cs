using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.InternalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Utility
{
    public class PublicHelper
    {
        public static DbMethodCallExpression MakeNextValueForSequenceDbExpression(PropertyDescriptor propertyDescriptor)
        {
            MethodInfo nextValueForSequenceMethod = UtilConstants.MethodInfo_Sql_NextValueForSequence.MakeGenericMethod(propertyDescriptor.PropertyType);
            List<DbExpression> arguments = new List<DbExpression>() { new DbConstantExpression(propertyDescriptor.Definition.SequenceName) };

            DbMethodCallExpression getNextValueForSequenceExp = new DbMethodCallExpression(null, nextValueForSequenceMethod, arguments);

            return getNextValueForSequenceExp;
        }
        public static object ConvertObjType(object obj, Type conversionType)
        {
            conversionType = conversionType.GetUnderlyingType();
            if (obj.GetType() != conversionType)
                return Convert.ChangeType(obj, conversionType);

            return obj;
        }

        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(Dictionary<TKey, TValue> source)
        {
            Dictionary<TKey, TValue> ret = Clone<TKey, TValue>(source, source.Count);
            return ret;
        }
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(Dictionary<TKey, TValue> source, int capacity)
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(capacity);

            foreach (var kv in source)
            {
                ret.Add(kv.Key, kv.Value);
            }

            return ret;
        }
    }
}
