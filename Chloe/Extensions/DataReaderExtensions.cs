using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Chloe.Utility;

namespace Chloe.Extensions
{
    public static class DataReaderExtensions
    {
        public static short Reader_GetInt16(this IDataReader reader, int ordinal)
        {
            return reader.GetInt16(ordinal);
        }

        public static short? Reader_GetInt16_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetInt16(ordinal);
        }

        public static int Reader_GetInt32(this IDataReader reader, int ordinal)
        {
            return reader.GetInt32(ordinal);
        }

        public static int? Reader_GetInt32_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetInt32(ordinal);
        }

        public static long Reader_GetInt64(this IDataReader reader, int ordinal)
        {
            return reader.GetInt64(ordinal);
        }

        public static long? Reader_GetInt64_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetInt64(ordinal);
        }

        public static decimal Reader_GetDecimal(this IDataReader reader, int ordinal)
        {
            return reader.GetDecimal(ordinal);
        }

        public static decimal? Reader_GetDecimal_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetDecimal(ordinal);
        }

        public static double Reader_GetDouble(this IDataReader reader, int ordinal)
        {
            return reader.GetDouble(ordinal);
        }

        public static double? Reader_GetDouble_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetDouble(ordinal);
        }

        public static float Reader_GetFloat(this IDataReader reader, int ordinal)
        {
            return reader.GetFloat(ordinal);
        }

        public static float? Reader_GetFloat_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetFloat(ordinal);
        }

        public static bool Reader_GetBoolean(this IDataReader reader, int ordinal)
        {
            return reader.GetBoolean(ordinal);
        }

        public static bool? Reader_GetBoolean_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetBoolean(ordinal);
        }

        public static DateTime Reader_GetDateTime(this IDataReader reader, int ordinal)
        {
            return reader.GetDateTime(ordinal);
        }

        public static DateTime? Reader_GetDateTime_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetDateTime(ordinal);
        }

        public static Guid Reader_GetGuid(this IDataReader reader, int ordinal)
        {
            return reader.GetGuid(ordinal);
        }

        public static Guid? Reader_GetGuid_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetGuid(ordinal);
        }

        public static byte Reader_GetByte(this IDataReader reader, int ordinal)
        {
            return reader.GetByte(ordinal);
        }

        public static byte? Reader_GetByte_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetByte(ordinal);
        }

        public static char Reader_GetChar(this IDataReader reader, int ordinal)
        {
            return reader.GetChar(ordinal);
        }

        public static char? Reader_GetChar_Nullable(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetChar(ordinal);
        }

        public static TimeSpan Reader_GetTimeSpan(this IDataReader reader, int ordinal)
        {
            object o = reader.GetValue(ordinal);
            if (o == DBNull.Value)
            {
                string name = reader.GetName(ordinal);
                throw new Exception(name + " 不可为空");
            }

            return (TimeSpan)o;
        }

        public static TimeSpan? Reader_GetTimeSpan_Nullable(this IDataReader reader, int ordinal)
        {
            object o = reader.GetValue(ordinal);
            if (o == DBNull.Value)
            {
                return null;
            }

            return (TimeSpan)o;
        }

        public static string Reader_GetString(this IDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
            {
                return null;
            }

            return reader.GetString(ordinal);
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

            return (byte[])o;
        }

        public static char[] Reader_GetChars(this IDataReader reader, int ordinal)
        {
            object o = reader.GetValue(ordinal);
            if (o == DBNull.Value)
            {
                return null;
            }

            return (char[])o;
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

            int value = reader.GetInt32(ordinal);
            T t = (T)Enum.ToObject(typeof(T), value);
            return t;
        }

    }
}
