using Chloe.Core;
using Chloe.DbExpressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.MySql
{
    partial class SqlGenerator : DbExpressionVisitor<DbExpression>
    {
        static string GenParameterName(int ordinal)
        {
            if (ordinal < CacheParameterNames.Count)
            {
                return CacheParameterNames[ordinal];
            }

            return ParameterPrefix + ordinal.ToString();
        }

        static Stack<DbExpression> GatherBinaryExpressionOperand(DbBinaryExpression exp)
        {
            DbExpressionType nodeType = exp.NodeType;

            Stack<DbExpression> items = new Stack<DbExpression>();
            items.Push(exp.Right);

            DbExpression left = exp.Left;
            while (left.NodeType == nodeType)
            {
                exp = (DbBinaryExpression)left;
                items.Push(exp.Right);
                left = exp.Left;
            }

            items.Push(left);
            return items;
        }
        static void EnsureMethodDeclaringType(DbMethodCallExpression exp, Type ensureType)
        {
            if (exp.Method.DeclaringType != ensureType)
                throw UtilExceptions.NotSupportedMethod(exp.Method);
        }
        static void EnsureMethod(DbMethodCallExpression exp, MethodInfo methodInfo)
        {
            if (exp.Method != methodInfo)
                throw UtilExceptions.NotSupportedMethod(exp.Method);
        }


        static void EnsureTrimCharArgumentIsSpaces(DbExpression exp)
        {
            var m = exp as DbMemberExpression;
            if (m == null)
                throw new NotSupportedException();

            DbParameterExpression p;
            if (!DbExpressionExtensions.TryParseToParameterExpression(m, out p))
            {
                throw new NotSupportedException();
            }

            var arg = p.Value;

            if (arg == null)
                throw new NotSupportedException();

            var chars = arg as char[];
            if (chars.Length != 1 || chars[0] != ' ')
            {
                throw new NotSupportedException();
            }
        }
        static bool TryGetCastTargetDbTypeString(Type sourceType, Type targetType, out string dbTypeString, bool throwNotSupportedException = true)
        {
            dbTypeString = null;

            sourceType = Utils.GetUnderlyingType(sourceType);
            targetType = Utils.GetUnderlyingType(targetType);

            if (sourceType == targetType)
                return false;

            if (targetType == UtilConstants.TypeOfDecimal)
            {
                //Casting to Decimal is not supported when missing the precision and scale information.I have no idea to deal with this case now.
                if (sourceType != UtilConstants.TypeOfInt16 && sourceType != UtilConstants.TypeOfInt32 && sourceType != UtilConstants.TypeOfInt64 && sourceType != UtilConstants.TypeOfByte)
                {
                    if (throwNotSupportedException)
                        throw new NotSupportedException(AppendNotSupportedCastErrorMsg(sourceType, targetType));
                    else
                        return false;
                }
            }

            if (CSharpType_DbType_Mappings.TryGetValue(targetType, out dbTypeString))
            {
                return true;
            }

            if (throwNotSupportedException)
                throw new NotSupportedException(AppendNotSupportedCastErrorMsg(sourceType, targetType));
            else
                return false;
        }
        static string AppendNotSupportedCastErrorMsg(Type sourceType, Type targetType)
        {
            return string.Format("Does not support the type '{0}' converted to type '{1}'.", sourceType.FullName, targetType.FullName);
        }

        static void DbFunction_DATEADD(SqlGenerator generator, string interval, DbMethodCallExpression exp)
        {
            //DATE_ADD(now(),INTERVAL 1 day),DATE_ADD(now(),INTERVAL 10 MINUTE)
            generator._sqlBuilder.Append("DATE_ADD(");
            exp.Object.Accept(generator);
            generator._sqlBuilder.Append(",INTERVAL ");
            exp.Arguments[0].Accept(generator);
            generator._sqlBuilder.Append(" ", interval);
            generator._sqlBuilder.Append(")");
        }
        static void DbFunction_DATEPART(SqlGenerator generator, string functionName, DbExpression exp)
        {
            generator._sqlBuilder.Append(functionName);
            generator._sqlBuilder.Append("(");
            exp.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        static void DbFunction_DATEDIFF(SqlGenerator generator, string interval, DbExpression startDateTimeExp, DbExpression endDateTimeExp)
        {
            //TIMESTAMPDIFF(HOUR,'2003-02-01 11:00','2003-02-01 12:00');
            generator._sqlBuilder.Append("TIMESTAMPDIFF(");
            generator._sqlBuilder.Append(interval);
            generator._sqlBuilder.Append(",");
            startDateTimeExp.Accept(generator);
            generator._sqlBuilder.Append(",");
            endDateTimeExp.Accept(generator);
            generator._sqlBuilder.Append(")");
        }

        #region AggregateFunction
        static void Func_Count(SqlGenerator generator)
        {
            generator._sqlBuilder.Append("COUNT(1)");
        }
        static void Func_LongCount(SqlGenerator generator)
        {
            generator._sqlBuilder.Append("COUNT(1)");
        }
        static void Func_Sum(SqlGenerator generator, DbExpression exp)
        {
            generator._sqlBuilder.Append("SUM(");
            exp.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        static void Func_Max(SqlGenerator generator, DbExpression exp)
        {
            generator._sqlBuilder.Append("MAX(");
            exp.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        static void Func_Min(SqlGenerator generator, DbExpression exp)
        {
            generator._sqlBuilder.Append("MIN(");
            exp.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        static void Func_Average(SqlGenerator generator, DbExpression exp)
        {
            generator._sqlBuilder.Append("AVG(");
            exp.Accept(generator);
            generator._sqlBuilder.Append(")");
        }
        #endregion

    }
}
