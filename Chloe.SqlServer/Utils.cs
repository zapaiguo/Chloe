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
        static List<Type> mapTypes;
        static Utils()
        {
            mapTypes = new List<Type>(17);
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
            mapTypes.Add(UtilConstants.TypeOfTimeSpan);
            mapTypes.Add(UtilConstants.TypeOfDateTimeOffset);
            mapTypes.Add(UtilConstants.TypeOfString);
            mapTypes.Add(UtilConstants.TypeOfObject);
            mapTypes.Add(UtilConstants.TypeOfByteArray);
            mapTypes.Add(UtilConstants.TypeOfCharArray);
            mapTypes.TrimExcess();
        }

        public static void CheckNull(object obj, string paramName = null)
        {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }
        public static bool IsNullable(Type type, out Type unType)
        {
            unType = Nullable.GetUnderlyingType(type);
            return unType != null;
        }
    }
}
