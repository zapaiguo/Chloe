using Chloe.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Extensions
{
    static class DataReaderConstant
    {
        #region
        internal static MethodInfo GetReaderMethod(Type type)
        {

            MethodInfo result;
            bool isNullable = false;
            Type underlyingType;
            if (Utils.IsNullable(type, out underlyingType))
            {
                isNullable = true;
                type = underlyingType;
            }

            if (type.IsEnum)
            {
                result = (isNullable ? Reader_GetEnum_Nullable : Reader_GetEnum).MakeGenericMethod(type);
                return result;
            }

            var typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Int16:
                    result = isNullable ? Reader_GetInt16_Nullable : Reader_GetInt16;
                    break;
                case TypeCode.Int32:
                    result = isNullable ? Reader_GetInt32_Nullable : Reader_GetInt32;
                    break;
                case TypeCode.Int64:
                    result = isNullable ? Reader_GetInt64_Nullable : Reader_GetInt64;
                    break;
                case TypeCode.Decimal:
                    result = isNullable ? Reader_GetDecimal_Nullable : Reader_GetDecimal;
                    break;
                case TypeCode.Double:
                    result = isNullable ? Reader_GetDouble_Nullable : Reader_GetDouble;
                    break;
                case TypeCode.Single:
                    result = isNullable ? Reader_GetFloat_Nullable : Reader_GetFloat;
                    break;
                case TypeCode.Boolean:
                    result = isNullable ? Reader_GetBoolean_Nullable : Reader_GetBoolean;
                    break;
                case TypeCode.DateTime:
                    result = isNullable ? Reader_GetDateTime_Nullable : Reader_GetDateTime;
                    break;
                case TypeCode.Byte:
                    result = isNullable ? Reader_GetByte_Nullable : Reader_GetByte;
                    break;
                case TypeCode.Char:
                    result = isNullable ? Reader_GetChar_Nullable : Reader_GetChar;
                    break;
                case TypeCode.String:
                    result = Reader_GetString;
                    break;
                default:
                    if (type == UtilConstants.TypeOfGuid)
                    {
                        result = isNullable ? Reader_GetGuid_Nullable : Reader_GetGuid;
                    }
                    else if (type == UtilConstants.TypeOfObject)
                    {
                        result = Reader_GetValue;
                    }
                    else
                    {
                        result = (isNullable ? Reader_GetValue_NullableT : Reader_GetValue_T).MakeGenericMethod(type);
                    }
                    break;
            }
            return result;
        }

        #region
        internal static readonly MethodInfo Reader_GetInt16 = typeof(DataReaderExtensions).GetMethod("Reader_GetInt16");
        internal static readonly MethodInfo Reader_GetInt16_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetInt16_Nullable");
        internal static readonly MethodInfo Reader_GetInt32 = typeof(DataReaderExtensions).GetMethod("Reader_GetInt32");
        internal static readonly MethodInfo Reader_GetInt32_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetInt32_Nullable");
        internal static readonly MethodInfo Reader_GetInt64 = typeof(DataReaderExtensions).GetMethod("Reader_GetInt64");
        internal static readonly MethodInfo Reader_GetInt64_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetInt64_Nullable");
        internal static readonly MethodInfo Reader_GetDecimal = typeof(DataReaderExtensions).GetMethod("Reader_GetDecimal");
        internal static readonly MethodInfo Reader_GetDecimal_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetDecimal_Nullable");
        internal static readonly MethodInfo Reader_GetDouble = typeof(DataReaderExtensions).GetMethod("Reader_GetDouble");
        internal static readonly MethodInfo Reader_GetDouble_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetDouble_Nullable");
        internal static readonly MethodInfo Reader_GetFloat = typeof(DataReaderExtensions).GetMethod("Reader_GetFloat");
        internal static readonly MethodInfo Reader_GetFloat_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetFloat_Nullable");
        internal static readonly MethodInfo Reader_GetBoolean = typeof(DataReaderExtensions).GetMethod("Reader_GetBoolean");
        internal static readonly MethodInfo Reader_GetBoolean_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetBoolean_Nullable");
        internal static readonly MethodInfo Reader_GetDateTime = typeof(DataReaderExtensions).GetMethod("Reader_GetDateTime");
        internal static readonly MethodInfo Reader_GetDateTime_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetDateTime_Nullable");
        internal static readonly MethodInfo Reader_GetGuid = typeof(DataReaderExtensions).GetMethod("Reader_GetGuid");
        internal static readonly MethodInfo Reader_GetGuid_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetGuid_Nullable");
        internal static readonly MethodInfo Reader_GetByte = typeof(DataReaderExtensions).GetMethod("Reader_GetByte");
        internal static readonly MethodInfo Reader_GetByte_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetByte_Nullable");
        internal static readonly MethodInfo Reader_GetChar = typeof(DataReaderExtensions).GetMethod("Reader_GetChar");
        internal static readonly MethodInfo Reader_GetChar_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetChar_Nullable");
        internal static readonly MethodInfo Reader_GetString = typeof(DataReaderExtensions).GetMethod("Reader_GetString");
        internal static readonly MethodInfo Reader_GetValue = typeof(DataReaderExtensions).GetMethod("Reader_GetValue");

        internal static readonly MethodInfo Reader_GetEnum = typeof(DataReaderExtensions).GetMethod("Reader_GetEnum");
        internal static readonly MethodInfo Reader_GetEnum_Nullable = typeof(DataReaderExtensions).GetMethod("Reader_GetEnum_Nullable");

        internal static readonly MethodInfo Reader_GetValue_T = typeof(DataReaderExtensions).GetMethod("Reader_GetValue_T");
        internal static readonly MethodInfo Reader_GetValue_NullableT = typeof(DataReaderExtensions).GetMethod("Reader_GetValue_NullableT");
        #endregion

        #endregion

    }
}
