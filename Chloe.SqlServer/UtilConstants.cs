using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.SqlServer
{
    static class UtilConstants
    {
        public static readonly Type[] EmptyTypeArray = new Type[0];

        public static readonly Type TypeOfVoid = typeof(void);

        public static readonly Type TypeOfInt16 = typeof(Int16);
        //public static readonly Type TypeOfInt16_Nullable = typeof(Int16?);

        public static readonly Type TypeOfInt32 = typeof(Int32);
        //public static readonly Type TypeOfInt32_Nullable = typeof(Int32?);

        public static readonly Type TypeOfInt64 = typeof(Int64);
        //public static readonly Type TypeOfInt64_Nullable = typeof(Int64?);

        public static readonly Type TypeOfDecimal = typeof(Decimal);
        //public static readonly Type TypeOfDecimal_Nullable = typeof(Decimal?);

        public static readonly Type TypeOfDouble = typeof(Double);
        //public static readonly Type TypeOfDouble_Nullable = typeof(Double?);

        public static readonly Type TypeOfSingle = typeof(Single);
        //public static readonly Type TypeOfSingle_Nullable = typeof(Single?);

        public static readonly Type TypeOfBoolean = typeof(Boolean);
        public static readonly Type TypeOfBoolean_Nullable = typeof(Boolean?);

        public static readonly Type TypeOfDateTime = typeof(DateTime);
        //public static readonly Type TypeOfDateTime_Nullable = typeof(DateTime?);

        public static readonly Type TypeOfGuid = typeof(Guid);
        //public static readonly Type TypeOfGuid_Nullable = typeof(Guid?);

        public static readonly Type TypeOfByte = typeof(Byte);
        //public static readonly Type TypeOfByte_Nullable = typeof(Byte?);

        public static readonly Type TypeOfChar = typeof(Char);
        //public static readonly Type TypeOfChar_Nullable = typeof(Char?);

        public static readonly Type TypeOfTimeSpan = typeof(TimeSpan);
        //public static readonly Type TypeOfTimeSpan_Nullable = typeof(TimeSpan?);

        //public static readonly Type TypeOfDateTimeOffset = typeof(DateTimeOffset);
        //public static readonly Type TypeOfDateTimeOffset_Nullable = typeof(DateTimeOffset?);

        public static readonly Type TypeOfString = typeof(String);
        public static readonly Type TypeOfObject = typeof(Object);
        public static readonly Type TypeOfByteArray = typeof(Byte[]);
        public static readonly Type TypeOfCharArray = typeof(Char[]);



        #region DbExpression constants

        public static readonly DbParameterExpression DbParameter_1 = DbExpression.Parameter(1);
        public static readonly DbConstantExpression DbConstant_1 = DbExpression.Constant(1);
        public static readonly DbConstantExpression DbConstant_0 = DbExpression.Constant(0);
        public static readonly DbConstantExpression DbConstant_True = DbExpression.Constant(true);
        public static readonly DbConstantExpression DbConstant_False = DbExpression.Constant(false);
        public static readonly DbConstantExpression DbConstant_Null_String = DbExpression.Constant(null, typeof(string));

        #endregion

        #region MemberInfo constants

        public static readonly PropertyInfo PropertyInfo_String_Length = typeof(string).GetProperty("Length");

        public static readonly PropertyInfo PropertyInfo_DateTime_Now = typeof(DateTime).GetProperty("Now");
        public static readonly PropertyInfo PropertyInfo_DateTime_UtcNow = typeof(DateTime).GetProperty("UtcNow");
        public static readonly PropertyInfo PropertyInfo_DateTime_Today = typeof(DateTime).GetProperty("Today");
        public static readonly PropertyInfo PropertyInfo_DateTime_Date = typeof(DateTime).GetProperty("Date");
        public static readonly PropertyInfo PropertyInfo_DateTime_Year = typeof(DateTime).GetProperty("Year");
        public static readonly PropertyInfo PropertyInfo_DateTime_Month = typeof(DateTime).GetProperty("Month");
        public static readonly PropertyInfo PropertyInfo_DateTime_Day = typeof(DateTime).GetProperty("Day");
        public static readonly PropertyInfo PropertyInfo_DateTime_Hour = typeof(DateTime).GetProperty("Hour");
        public static readonly PropertyInfo PropertyInfo_DateTime_Minute = typeof(DateTime).GetProperty("Minute");
        public static readonly PropertyInfo PropertyInfo_DateTime_Second = typeof(DateTime).GetProperty("Second");
        public static readonly PropertyInfo PropertyInfo_DateTime_Millisecond = typeof(DateTime).GetProperty("Millisecond");
        public static readonly PropertyInfo PropertyInfo_DateTime_DayOfWeek = typeof(DateTime).GetProperty("DayOfWeek");

        public static readonly PropertyInfo PropertyInfo_TimeSpan_TotalDays = typeof(TimeSpan).GetProperty("TotalDays");
        public static readonly PropertyInfo PropertyInfo_TimeSpan_TotalHours = typeof(TimeSpan).GetProperty("TotalHours");
        public static readonly PropertyInfo PropertyInfo_TimeSpan_TotalMinutes = typeof(TimeSpan).GetProperty("TotalMinutes");
        public static readonly PropertyInfo PropertyInfo_TimeSpan_TotalSeconds = typeof(TimeSpan).GetProperty("TotalSeconds");
        public static readonly PropertyInfo PropertyInfo_TimeSpan_TotalMilliseconds = typeof(TimeSpan).GetProperty("TotalMilliseconds");

        public static readonly MethodInfo MethodInfo_String_Concat_String_String = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
        public static readonly MethodInfo MethodInfo_String_Concat_Object_Object = typeof(string).GetMethod("Concat", new Type[] { typeof(object), typeof(object) });

        public static readonly MethodInfo MethodInfo_DateTime_Subtract_DateTime = typeof(DateTime).GetMethod("Subtract", new Type[] { typeof(DateTime) });

        #endregion

    }
}
