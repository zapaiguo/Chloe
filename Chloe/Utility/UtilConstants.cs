using Chloe.DbExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chloe.Utility
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

        public static readonly Type TypeOfString = typeof(String);
        public static readonly Type TypeOfObject = typeof(Object);
        public static readonly Type TypeOfByteArray = typeof(Byte[]);
        public static readonly Type TypeOfCharArray = typeof(Char[]);



        public static ConstantExpression Constant_True = Expression.Constant(true);
        //public static UnaryExpression Convert_TrueToNullable = Expression.Convert(Expression.Constant(true), typeof(Boolean?));
        public static ConstantExpression Constant_False = Expression.Constant(false);
        public static UnaryExpression Convert_FalseToNullable = Expression.Convert(Expression.Constant(false), typeof(Boolean?));

        public static DbConstantExpression DbConstant_True = DbExpression.Constant(true);
        //public static DbConstantExpression DbConstant_False = DbExpression.Constant(false);
        public static DbEqualExpression DbEqual_TrueEqualFalse = DbExpression.Equal(DbExpression.Constant(true), DbExpression.Constant(false));
    }
}
