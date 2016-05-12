using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chloe.Utility
{
    internal static class Utils
    {
        static List<Type> MapTypes;
        static Utils()
        {
            var mapTypes = new List<Type>();
            mapTypes.Add(UtilConstants.TypeOfInt16);
            mapTypes.Add(UtilConstants.TypeOfInt32);
            mapTypes.Add(UtilConstants.TypeOfInt64);
            mapTypes.Add(UtilConstants.TypeOfDecimal);
            mapTypes.Add(UtilConstants.TypeOfDouble);
            mapTypes.Add(UtilConstants.TypeOfSingle);
            mapTypes.Add(UtilConstants.TypeOfBoolean);
            mapTypes.Add(UtilConstants.TypeOfDateTime);
            mapTypes.Add(UtilConstants.TypeOfGuid);
            mapTypes.Add(UtilConstants.TypeOfByte);
            mapTypes.Add(UtilConstants.TypeOfChar);
            mapTypes.Add(UtilConstants.TypeOfString);
            mapTypes.Add(UtilConstants.TypeOfObject);

            mapTypes.TrimExcess();

            MapTypes = mapTypes;
        }

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

        public static bool IsMapType(Type type)
        {
            Type unType;
            if (!Utils.IsNullable(type, out unType))
                unType = type;

            if (unType.IsEnum)
                return true;

            return MapTypes.Contains(unType);
        }

        public static string GenerateUniqueColumnAlias(DbSqlQueryExpression sqlQuery, string defaultAlias = "C")
        {
            string alias = defaultAlias;
            int i = 0;
            while (sqlQuery.Columns.Any(a => string.Equals(a.Alias, alias, StringComparison.OrdinalIgnoreCase)))
            {
                alias = defaultAlias + i.ToString();
                i++;
            }

            return alias;
        }

    }

}
