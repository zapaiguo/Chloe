using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.SqlServer
{
    internal static class Utils
    {
        public static void CheckNull(object obj, string paramName = null)
        {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }
        public static bool IsNullable(Type type)
        {
            Type unType;
            return IsNullable(type, out unType);
        }
        public static bool IsNullable(Type type, out Type unType)
        {
            unType = Nullable.GetUnderlyingType(type);
            return unType != null;
        }
    }
}
