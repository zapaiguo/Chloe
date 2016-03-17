using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chloe.Utility;

namespace Chloe.Extensions
{
    //33
    public static class DataReaderExtensions
    {
        public static short Reader_GetInt16(this IDataReader reader, int ordinal)
        {
            //if (reader.IsDBNull(ordinal))
            //{
            //    string name = reader.GetName(ordinal);
            //    throw new Exception(name + " 不可为空");
            //}
            //Convert.
            try
            {
                return reader.GetInt16(ordinal);
            }
            catch (SqlNullValueException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空", e);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static short? Reader_GetInt16_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return reader.GetInt16(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static int Reader_GetInt32(this IDataReader reader, int ordinal)
        {
            //if (reader.IsDBNull(ordinal))
            //{
            //    string name = reader.GetName(ordinal);
            //    throw new Exception(name + " 不可为空");
            //}
            try
            {
                return reader.GetInt32(ordinal);
            }
            catch (SqlNullValueException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空", e);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 映射错误", e);
            }
        }

        public static int? Reader_GetInt32_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return reader.GetInt32(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static long Reader_GetInt64(this IDataReader reader, int ordinal)
        {
            //if (reader.IsDBNull(ordinal))
            //{
            //    string name = reader.GetName(ordinal);
            //    throw new Exception(name + " 不可为空");
            //}
            try
            {
                return reader.GetInt64(ordinal);
            }
            catch (SqlNullValueException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空", e);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static long? Reader_GetInt64_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return reader.GetInt64(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static decimal Reader_GetDecimal(this IDataReader reader, int ordinal)
        {
            //if (reader.IsDBNull(ordinal))
            //{
            //    string name = reader.GetName(ordinal);
            //    throw new Exception(name + " 不可为空");
            //}
            try
            {
                return reader.GetDecimal(ordinal);
            }
            catch (SqlNullValueException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空", e);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static decimal? Reader_GetDecimal_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return reader.GetDecimal(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static double Reader_GetDouble(this IDataReader reader, int ordinal)
        {
            //if (reader.IsDBNull(ordinal))
            //{
            //    string name = reader.GetName(ordinal);
            //    throw new Exception(name + " 不可为空");
            //}
            try
            {
                return reader.GetDouble(ordinal);
            }
            catch (SqlNullValueException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空", e);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static double? Reader_GetDouble_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return reader.GetDouble(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static float Reader_GetFloat(this IDataReader reader, int ordinal)
        {
            //if (reader.IsDBNull(ordinal))
            //{
            //    string name = reader.GetName(ordinal);
            //    throw new Exception(name + " 不可为空");
            //}

            try
            {
                return reader.GetFloat(ordinal);
            }
            catch (SqlNullValueException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空", e);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static float? Reader_GetFloat_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return reader.GetFloat(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static bool Reader_GetBoolean(this IDataReader reader, int ordinal)
        {
            //if (reader.IsDBNull(ordinal))
            //{
            //    string name = reader.GetName(ordinal);
            //    throw new Exception(name + " 不可为空");
            //}

            try
            {
                if (reader.GetFieldType(ordinal) != UtilConstants.TypeOfBoolean)
                {
                    return Convert.ToBoolean(reader.GetValue(ordinal));
                }
                return reader.GetBoolean(ordinal);
            }
            catch (SqlNullValueException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空", e);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static bool? Reader_GetBoolean_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                if (reader.GetFieldType(ordinal) != UtilConstants.TypeOfBoolean)
                {
                    return Convert.ToBoolean(reader.GetValue(ordinal));
                }
                return reader.GetBoolean(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static DateTime Reader_GetDateTime(this IDataReader reader, int ordinal)
        {
            //if (reader.IsDBNull(ordinal))
            //{
            //    string name = reader.GetName(ordinal);
            //    throw new Exception(name + " 不可为空");
            //}

            try
            {
                return reader.GetDateTime(ordinal);
            }
            catch (SqlNullValueException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空", e);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static DateTime? Reader_GetDateTime_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return reader.GetDateTime(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static Guid Reader_GetGuid(this IDataReader reader, int ordinal)
        {
            //if (reader.IsDBNull(ordinal))
            //{
            //    string name = reader.GetName(ordinal);
            //    throw new Exception(name + " 不可为空");
            //}

            try
            {
                return reader.GetGuid(ordinal);
            }
            catch (SqlNullValueException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空", e);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static Guid? Reader_GetGuid_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return reader.GetGuid(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static byte Reader_GetByte(this IDataReader reader, int ordinal)
        {
            //if (reader.IsDBNull(ordinal))
            //{
            //    string name = reader.GetName(ordinal);
            //    throw new Exception(name + " 不可为空");
            //}

            try
            {
                return reader.GetByte(ordinal);
            }
            catch (SqlNullValueException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空", e);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static byte? Reader_GetByte_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return reader.GetByte(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static char Reader_GetChar(this IDataReader reader, int ordinal)
        {
            //if (reader.IsDBNull(ordinal))
            //{
            //    string name = reader.GetName(ordinal);
            //    throw new Exception(name + " 不可为空");
            //}
            try
            {
                return reader.GetChar(ordinal);
            }
            catch (SqlNullValueException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空", e);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static char? Reader_GetChar_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return reader.GetChar(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static TimeSpan Reader_GetTimeSpan(this IDataReader reader, int ordinal)
        {
            object o = reader.GetValue(ordinal);
            if (o == DBNull.Value)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空");
            }

            try
            {
                return (TimeSpan)o;
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static TimeSpan? Reader_GetTimeSpan_Nullable(this IDataReader reader, int ordinal)
        {
            object o = reader.GetValue(ordinal);
            if (o == DBNull.Value)
            {
                return null;
            }

            try
            {
                return (TimeSpan)o;
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static DateTimeOffset Reader_GetDateTimeOffset(this IDataReader reader, int ordinal)
        {
            object o = reader.GetValue(ordinal);
            if (o == DBNull.Value)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空");
            }

            try
            {
                return (DateTimeOffset)o;
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static DateTimeOffset? Reader_GetDateTimeOffset_Nullable(this IDataReader reader, int ordinal)
        {
            object o = reader.GetValue(ordinal);
            if (o == DBNull.Value)
            {
                return null;
            }

            try
            {
                return (DateTimeOffset)o;
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static string Reader_GetString(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                return reader.GetString(ordinal);
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static object Reader_GetValue(this IDataReader reader, int ordinal)
        {
            object o = reader.GetValue(ordinal);
            if (o == DBNull.Value)
            {
                return null;
            }

            return o;
        }

        public static byte[] Reader_GetBytes(this IDataReader reader, int ordinal)
        {
            object o = reader.GetValue(ordinal);
            if (o == DBNull.Value)
            {
                return null;
            }

            try
            {
                return (byte[])o;
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static char[] Reader_GetChars(this IDataReader reader, int ordinal)
        {
            object o = reader.GetValue(ordinal);
            if (o == DBNull.Value)
            {
                return null;
            }

            try
            {
                return (char[])o;
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

        public static T Reader_GetEnum<T>(this IDataReader reader, int ordinal) where T : struct
        {
            int value = reader.GetInt32(ordinal);
            T t = (T)Enum.ToObject(typeof(T), value);
            return t;
        }

        public static T? Reader_GetEnum_Nullable<T>(this IDataReader reader, int ordinal) where T : struct
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            try
            {
                int value = reader.GetInt32(ordinal);
                T t = (T)Enum.ToObject(typeof(T), value);
                return t;
            }
            catch (InvalidCastException e)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 类型映射错误", e);
            }
        }

    }
}
